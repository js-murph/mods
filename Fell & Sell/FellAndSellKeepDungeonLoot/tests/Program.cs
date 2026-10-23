using FellAndSell;
using HarmonyLib;
using Il2Cpp;
using Il2CppGame;
using MelonLoader;

var harmony = new Harmony("FellAndSell.KeepDungeonLoot.Checks");
harmony.PatchAll(typeof(KeepDungeonLoot).Assembly);
int passed = 0;
void Expect(bool value) { if (!value) throw new Exception("Check failed"); }
void Check(string name, Action check) { check(); Console.WriteLine("PASS " + name); passed++; }
PlayerStats Reset(bool dungeon = true, bool enabled = true, bool forest = false)
{
    GameSceneManager.IsDungeon = dungeon;
    GameSceneManager.IsForest = forest;
    InventoryManager.Instance = new(); PlayerEquipment.Instance = new(); ShopManager.Instance = new();
    InventoryManager.Instance.Backpack.Add(new("Ore", 17, 0, Array.Empty<Item>()));
    InventoryManager.Instance.Backpack.Add(new("Chest", 1, 0, new[] { new Item("Gem", 3, 0, Array.Empty<Item>()) }));
    InventoryManager.Instance.Equipped.Add(new("Sword", 1, 4, Array.Empty<Item>()));
    InventoryManager.Instance.Quick.Add(new("Potion", 7, 0, Array.Empty<Item>()));
    new KeepDungeonLoot().OnInitializeMelon(); MelonPreferences.Enabled.Value = enabled;
    return new();
}
void ExpectCleared()
{
    Expect(InventoryManager.Instance.Backpack.Count == 0 && InventoryManager.Instance.Equipped.Count == 0 &&
        InventoryManager.Instance.Quick.Count == 0 && ShopManager.Instance.Gold == 0 && !PlayerEquipment.Instance.HasVisuals);
}
void ClearNormally()
{
    InventoryManager.Instance.ClearAllItems(false, false);
    PlayerEquipment.Instance.DropAllEquippedVisuals(); ShopManager.Instance.ResetGold();
}

try
{
    Check("dungeon and forest deaths retain every slot, metadata and gold through save/return, on both difficulty branches", () =>
    {
        foreach (bool forest in new[] { false, true })
        foreach (bool keepEquipped in new[] { false, true })
        {
            var p = Reset(dungeon: !forest, forest: forest); p.KeepEquippedOnDeath = keepEquipped;
            string before = InventoryManager.Instance.Snapshot(); var routine = p.DeathRoutine();
            Expect(routine.MoveNext()); Expect(p.PlayedDeathAnimation);
            Expect(InventoryManager.Instance.Snapshot() == before && PlayerEquipment.Instance.HasVisuals && ShopManager.Instance.Gold == 137);
            Expect(!routine.MoveNext());
            Expect(p.ReturnedToTown && p.SavedInventory == before && p.SavedGold == 137);
        }
    });
    Check("disabled mod leaves native death losses and return intact", () =>
    {
        foreach (bool forest in new[] { false, true })
        {
            var p = Reset(dungeon: !forest, forest: forest, enabled: false); var routine = p.DeathRoutine();
            Expect(routine.MoveNext()); ExpectCleared(); Expect(!routine.MoveNext() && p.ReturnedToTown);
        }
    });
    Check("deaths outside dungeons and forests keep their normal penalties", () =>
    {
        var p = Reset(dungeon: false); var routine = p.DeathRoutine();
        Expect(routine.MoveNext()); ExpectCleared(); Expect(!routine.MoveNext() && p.ReturnedToTown);
    });
    Check("new-game and normal clears are not blocked, including between coroutine yields", () =>
    {
        var p = Reset(); ClearNormally(); ExpectCleared();
        p = Reset(); var routine = p.DeathRoutine(); Expect(routine.MoveNext());
        ClearNormally(); ExpectCleared();
        Expect(!routine.MoveNext() && p.SavedGold == 0);
    });
    Check("exceptions propagate without leaving inventory protection active", () =>
    {
        var p = Reset(); p.ThrowAfterLosses = true;
        var routine = p.DeathRoutine(); bool threw = false;
        try { routine.MoveNext(); } catch (InvalidOperationException) { threw = true; }
        Expect(threw && InventoryManager.Instance.Backpack.Count == 2 && ShopManager.Instance.Gold == 137);
        ClearNormally(); ExpectCleared();
    });
    Check("repeated runs do not duplicate items or carry protection into town", () =>
    {
        var p = Reset(); string before = InventoryManager.Instance.Snapshot();
        for (int i = 0; i < 3; i++)
        {
            GameSceneManager.IsDungeon = true;
            var routine = p.DeathRoutine(); Expect(routine.MoveNext()); Expect(!routine.MoveNext());
            Expect(p.SavedInventory == before && ShopManager.Instance.Gold == 137);
        }
        ClearNormally(); ExpectCleared();
    });
    Console.WriteLine($"{passed} checks passed: actual mod + Harmony, simulated game. Native gameplay/save verification still required.");
}
finally { harmony.UnpatchSelf(); }
