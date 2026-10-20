using System.Text.Json;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(FellAndSell.AutoRestock), "Auto Restock Tables", "1.0.0", "Local Mod")]
[assembly: MelonGame("Art Games Studio SA", "Fell & Sell")]
[assembly: MelonGame("Art Games Studio SA", "Fell _ Sell")]

namespace FellAndSell;

public sealed class AutoRestock : MelonMod
{
    private const string SaveKey = "FellAndSell.AutoRestock.v1";
    private sealed class Table
    {
        public ShelfSlot Slot;
        public TableRule Rule;
    }

    private readonly Dictionary<int, Table> tables = new();
    private PlayerInteraction player;
    private float nextCheck;
    private bool faulted;
    internal static AutoRestock Instance;

    public override void OnInitializeMelon()
    {
        Instance = this;
        LoggerInstance.Msg("Ready. Look at a stocked table: F6 toggles Always, F7 toggles Demand only (Normal/High). Stock comes from your backpack.");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName) => player = null;

    public override void OnUpdate()
    {
        if (faulted)
            return;
        try
        {
            var saves = SaveManager.Instance;
            if (Time.timeScale <= 0 || saves == null || saves.IsSaving ||
                saves.isRestoringFromSave || saves.isRestoringSceneState)
                return;

            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.f6Key.wasPressedThisFrame || keyboard.f7Key.wasPressedThisFrame))
                HandleHotkey(keyboard.f6Key.wasPressedThisFrame ? RestockMode.Always : RestockMode.DemandOnly);

            if (Time.unscaledTime < nextCheck)
                return;
            nextCheck = Time.unscaledTime + 0.5f;

            var inventory = InventoryManager.Instance;
            var shop = ShopManager.Instance;
            var shelfManager = ShelfManager.Instance;
            if (inventory == null || shop == null || shelfManager == null)
                return;

            // Leave stock alone while a UI may hold a source slot for a manual transfer.
            if ((PlayerInputHandler.Instance != null && PlayerInputHandler.Instance.IsPlayerActionBlocked()) ||
                shelfManager.IsShelfModeActive ||
                (FurniturePlacementManager.Instance != null && FurniturePlacementManager.Instance.IsPlacing))
                return;

            foreach (var pair in tables.ToArray())
            {
                var table = pair.Value;
                if (table.Slot == null)
                {
                    tables.Remove(pair.Key);
                    continue;
                }
                if (!table.Slot.isActiveAndEnabled)
                    continue;
                CaptureItem(table);
                Restock(table, inventory, shop, shelfManager);
            }
        }
        catch (Exception error)
        {
            faulted = true;
            LoggerInstance.Error($"Auto-restock disabled for this session; no automatic retries: {error}");
        }
    }

    private void HandleHotkey(RestockMode requested)
    {
        var input = PlayerInputHandler.Instance;
        if (input == null || input.IsPlayerActionBlocked())
            return;
        if (player == null)
            player = UnityEngine.Object.FindObjectOfType<PlayerInteraction>();
        var interactable = player?.currentInteractable;
        if (interactable == null)
            return;
        var slot = interactable.TryCast<ShelfSlot>() ?? SingleSlot(interactable.TryCast<ItemPickup>());
        if (slot == null || !slot.isActiveAndEnabled)
            return;

        int id = slot.GetInstanceID();
        if (tables.TryGetValue(id, out var table))
        {
            if (table.Rule.Mode == requested)
                tables.Remove(id);
            else
                table.Rule.Mode = requested;
        }
        else
        {
            // An unconfigured empty table has no item to remember.
            if (slot.currentItem == null || slot.quantity <= 0 || slot.basePrice <= 0)
                return;
            table = new Table { Slot = slot, Rule = new TableRule { Mode = requested } };
            CaptureItem(table);
            tables.Add(id, table);
        }
        player.UpdateUI(true, interactable, true);
        LoggerInstance.Msg($"Table restock: {(tables.TryGetValue(id, out table) ? table.Rule.Mode : RestockMode.Off)}");
    }

    private static ShelfSlot SingleSlot(ItemPickup pickup)
    {
        if (pickup == null || pickup.isFurnitureScroll)
            return null;
        var slots = pickup.GetComponentsInChildren<ShelfSlot>();
        // Never guess which slot on a multi-slot shelf the player meant.
        return slots.Length == 1 ? slots[0] : null;
    }

    private static void CaptureItem(Table table)
    {
        var slot = table.Slot;
        if (slot.currentItem != null && slot.basePrice > 0)
        {
            table.Rule.ItemName = slot.currentItem.name;
            table.Rule.BasePrice = slot.basePrice;
        }
    }

    private static void Restock(Table table, InventoryManager inventory, ShopManager shop, ShelfManager shelfManager)
    {
        var slot = table.Slot;
        var rule = table.Rule;
        var item = ItemDatabaseUtility.GetItemByName(rule.ItemName);
        if (item == null || rule.BasePrice <= 0 || !slot.CanAcceptItem(item) ||
            (slot.currentItem != null && slot.currentItem != item))
            return;
        if (rule.Mode == RestockMode.DemandOnly)
        {
            var demand = shop.GetDemandLevel(item);
            if (demand != ShopManager.DemandLevel.Normal && demand != ShopManager.DemandLevel.High)
                return;
        }

        int capacity = Math.Min(slot.MaxCapacity, item.itemSize == ItemSize.Big ? 1 : shelfManager.maxSmallItemStack);
        if (slot.quantity >= capacity)
            return;

        // Inventory callbacks may remove/reorder slots, so iterate a managed snapshot.
        var sources = new InventorySlot[inventory.inventorySlots.Count];
        for (int i = 0; i < sources.Length; i++)
            sources[i] = inventory.inventorySlots[i];
        foreach (var source in sources)
        {
            if (slot.quantity >= capacity)
                break;
            if (source == null || !inventory.inventorySlots.Contains(source))
                continue;
            source.EnsureItemResolved();
            if (source.item != item || source.quantity <= 0 || source.isEquipped || source.upgradeLevel != 0 ||
                inventory.IsEquipmentSlot(source) || inventory.IsQuickSlot(source) ||
                (source.storedItems != null && source.storedItems.Count > 0))
                continue;

            int amount = Math.Min(source.quantity, capacity - slot.quantity);
            int before = slot.quantity;
            int accepted;
            try
            {
                // The native return value is the accepted count, not the unplaced remainder.
                // Pass the unmodified base price so demand/furniture bonuses are applied once.
                accepted = slot.PlaceItem(item, amount, rule.BasePrice, false);
            }
            catch
            {
                // PlaceItem changes quantity before animating. Account for a partial success
                // if animation/UI throws, then let the outer handler stop the session.
                int placed = Math.Clamp(slot.quantity - before, 0, amount);
                if (slot.currentItem == item && placed > 0)
                    inventory.RemoveItemAmount(source, placed);
                throw;
            }
            if (accepted > 0)
                inventory.RemoveItemAmount(source, accepted);
        }
    }

    private void Describe(ShelfSlot slot, ref string text)
    {
        if (slot == null)
            return;
        string mode = "Off";
        if (tables.TryGetValue(slot.GetInstanceID(), out var table))
            mode = table.Rule.Mode == RestockMode.Always ? "Always" : "Demand only (Normal/High)";
        if (faulted)
            mode = "Stopped (see MelonLoader log)";
        text += $"\n<size=75%>Restock: {mode}\nF6 Always / F7 Demand (press again: Off)</size>";
    }

    private void SaveRules(string file)
    {
        var records = new List<TableRule>();
        foreach (var table in tables.Values)
        {
            if (table.Slot == null || !table.Slot.gameObject.scene.IsValid())
                continue;
            CaptureItem(table);
            var position = table.Slot.transform.position;
            var rule = table.Rule;
            rule.Scene = table.Slot.gameObject.scene.name;
            rule.SlotName = table.Slot.name;
            rule.X = position.x;
            rule.Y = position.y;
            rule.Z = position.z;
            records.Add(rule);
        }
        ES3.Save(SaveKey, JsonSerializer.Serialize(records), file);
    }

    private void LoadRules(string file)
    {
        tables.Clear();
        var records = JsonSerializer.Deserialize<List<TableRule>>(ES3.Load(SaveKey, file, "[]"));
        if (records == null || ShelfSlot.AllSlots == null)
            return;
        foreach (var rule in records)
        {
            if (rule == null || (rule.Mode != RestockMode.Always && rule.Mode != RestockMode.DemandOnly) ||
                string.IsNullOrEmpty(rule.ItemName) || rule.BasePrice <= 0)
                continue;
            ShelfSlot match = null;
            foreach (var slot in ShelfSlot.AllSlots)
            {
                if (slot == null || slot.name != rule.SlotName || slot.gameObject.scene.name != rule.Scene)
                    continue;
                var position = slot.transform.position;
                float dx = position.x - rule.X, dy = position.y - rule.Y, dz = position.z - rule.Z;
                if (dx * dx + dy * dy + dz * dz >= 0.0025f)
                    continue;
                // Position-based restoration follows the vanilla save format, but ambiguous
                // matches are left Off rather than enabling the wrong table.
                if (match != null)
                {
                    match = null;
                    break;
                }
                match = slot;
            }
            if (match != null && !tables.ContainsKey(match.GetInstanceID()))
                tables.Add(match.GetInstanceID(), new Table { Slot = match, Rule = rule });
        }
    }

    private static void Safely(Action action)
    {
        try { action(); }
        catch (Exception error)
        {
            // A mod hook must never prevent normal save/load or interaction code from running.
            if (Instance != null)
            {
                Instance.faulted = true;
                Instance.LoggerInstance.Error($"Auto-restock hook failed; automation stopped: {error}");
            }
        }
    }

    [HarmonyPatch(typeof(ShelfSlot), nameof(ShelfSlot.GetObjectText))]
    private static class SlotLabel
    {
        private static void Postfix(ShelfSlot __instance, ref string __result)
        {
            string text = __result;
            Safely(() => Instance?.Describe(__instance, ref text));
            __result = text;
        }
    }

    [HarmonyPatch(typeof(ItemPickup), nameof(ItemPickup.GetObjectText))]
    private static class TableLabel
    {
        private static void Postfix(ItemPickup __instance, ref string __result)
        {
            string text = __result;
            Safely(() => Instance?.Describe(SingleSlot(__instance), ref text));
            __result = text;
        }
    }

    [HarmonyPatch(typeof(ShelfSlot), nameof(ShelfSlot.Clear))]
    private static class RememberBeforeEmpty
    {
        private static void Prefix(ShelfSlot __instance) => Safely(() =>
        {
            if (Instance != null && Instance.tables.TryGetValue(__instance.GetInstanceID(), out var table))
                CaptureItem(table);
        });
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveShelfSlots))]
    private static class SaveFlags
    {
        private static void Postfix(string file) => Safely(() => Instance?.SaveRules(file));
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadShelfSlots))]
    private static class LoadFlags
    {
        private static void Postfix(string file) => Safely(() => Instance?.LoadRules(file));
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.PrepareForNewGame))]
    private static class NewGame
    {
        private static void Prefix() => Instance?.tables.Clear();
    }
}
