using System.Globalization;
using FellAndSell;
using HarmonyLib;
using Il2Cpp;
using Il2CppGame;
using MelonLoader;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
var harmony = new Harmony("FellAndSell.NoDungeonWeight.Checks");
harmony.PatchAll(typeof(NoDungeonWeight).Assembly);
int passed = 0;
void Expect(bool value) { if (!value) throw new Exception("Check failed"); }
void Check(string name, Action test) { test(); Console.WriteLine("PASS " + name); passed++; }
InventoryManager Reset(bool dungeon = true, bool enabled = true, bool forest = false)
{
    GameSceneManager.IsDungeon = dungeon;
    GameSceneManager.IsForest = forest;
    new NoDungeonWeight().OnInitializeMelon();
    MelonPreferences.Enabled.Value = enabled;
    return InventoryManager.Instance = new();
}

try
{
    Check("dungeon pickup checks and insertion accept heavy nested loot without changing its weight or metadata", () =>
    {
        var inventory = Reset();
        var chest = new Item(5f, 1, 4, new[] { new Item(13f, 7), new Item(2.5f, 3) });
        Expect(inventory.CanCarryItem(chest) && inventory.AddItem(chest));
        Expect(inventory.CurrentWeight == 103.5f && inventory.Items.Single() == chest);
        Expect(chest.Weight == 5f && chest.Upgrade == 4 && chest.Contents[0].Quantity == 7);
        // Reject fixed high-capacity approximations: this load exceeds any practical cap.
        var huge = new Item(float.MaxValue / 4, 1);
        Expect(inventory.CanCarryItem(huge) && inventory.AddItem(huge));
        Expect(inventory.maxWeight == 20f && inventory.EquipmentBonus == 7f && inventory.DungeonBonus == 3f);
    });
    Check("invalid pickups remain rejected even when weight is unlimited", () =>
    {
        var inventory = Reset();
        foreach (var item in new Item[] { null, new(1f, 0), new(1f, -2) })
            Expect(!inventory.CanCarryItem(item) && !inventory.AddItem(item));
        Expect(inventory.Items.Count == 0);
    });
    Check("town, forest and disabled dungeon retain capacity bonuses and the exact weight boundary", () =>
    {
        foreach (var scene in new[] { "town", "forest", "disabled dungeon" })
        {
            var inventory = Reset(dungeon: scene == "disabled dungeon", enabled: scene != "disabled dungeon", forest: scene == "forest");
            Expect(inventory.EffectiveMaxWeight == 30f);
            Expect(inventory.AddItem(new(9f, 3)));
            Expect(inventory.CanCarryItem(new(3f, 1)) && inventory.AddItem(new(3f, 1)));
            Expect(!inventory.CanCarryItem(new(0.25f, 1)) && !inventory.AddItem(new(0.25f, 1)));
            Expect(inventory.CurrentWeight == 30f);
        }
    });
    Check("exit and disable restore capacity and UI without discarding overweight loot", () =>
    {
        foreach (bool disable in new[] { false, true })
        {
            var inventory = Reset(); var ui = new InventoryUI();
            Expect(inventory.AddItem(new(12.5f, 5)));
            ui.UpdateWeightUI();
            Expect(ui.weightText.text == "62.5 / Unlimited");
            Expect(ui.weightSlider.minValue == 0f && ui.weightSlider.maxValue == 1f && ui.weightSlider.value == 0f);
            if (disable) MelonPreferences.Enabled.Value = false;
            else GameSceneManager.IsDungeon = false;
            ui.UpdateWeightUI();
            Expect(inventory.CurrentWeight == 62.5f && inventory.EffectiveMaxWeight == 30f);
            Expect(!inventory.AddItem(new(1f, 1)));
            Expect(ui.weightText.text == "62.5 / 30.0" && ui.weightSlider.maxValue == 30f && ui.weightSlider.value == 30f);
            GameSceneManager.IsDungeon = true; MelonPreferences.Enabled.Value = true;
            Expect(inventory.AddItem(new(1f, 1)) && inventory.CurrentWeight == 63.5f);
        }
    });
    Check("empty inventory and absent UI components are safe", () =>
    {
        Reset(); var ui = new InventoryUI(); ui.UpdateWeightUI();
        Expect(ui.weightText.text == "0.0 / Unlimited" && ui.weightSlider.value == 0f);
        ui.weightSlider = null; ui.UpdateWeightUI();
        Expect(ui.weightText.text == "0.0 / Unlimited");
        ui.weightText = null; ui.UpdateWeightUI();
        InventoryManager.Instance = null; ui.UpdateWeightUI();
    });
    Console.WriteLine($"{passed} checks passed: actual mod + Harmony, simulated game. Native gameplay and rendered UI remain unverified.");
}
finally { harmony.UnpatchSelf(); }
