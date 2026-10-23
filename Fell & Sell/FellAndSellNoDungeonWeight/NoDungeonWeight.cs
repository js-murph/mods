using HarmonyLib;
using Il2Cpp;
using Il2CppGame;
using MelonLoader;

[assembly: MelonInfo(typeof(FellAndSell.NoDungeonWeight), "No Dungeon Weight Limit", "1.0.0", "Local Mod")]
[assembly: MelonGame("Art Games Studio SA", "Fell & Sell")]
[assembly: MelonGame("Art Games Studio SA", "Fell _ Sell")]

namespace FellAndSell;

public sealed class NoDungeonWeight : MelonMod
{
    private static MelonPreferences_Entry<bool> enabled;
    private static bool Active => enabled?.Value == true && GameSceneManager.IsDungeon;

    public override void OnInitializeMelon()
    {
        enabled = MelonPreferences.CreateCategory("NoDungeonWeight")
            .CreateEntry("Enabled", true, description: "Remove the carrying weight limit inside dungeons only.");
        LoggerInstance.Msg("Ready. Dungeon carrying weight is unlimited; item weights and limits outside dungeons are unchanged.");
    }

    [HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.EffectiveMaxWeight), MethodType.Getter)]
    private static class UnlimitedCapacity
    {
        private static void Postfix(ref float __result)
        {
            // AddItem, CanCarryItem, crafting and slot transfers share this getter.
            // Change only its result, never the saved base capacity or item weights.
            if (Active) __result = float.PositiveInfinity;
        }
    }

    [HarmonyPatch(typeof(InventoryUI), nameof(InventoryUI.UpdateWeightUI))]
    private static class UnlimitedWeightDisplay
    {
        private static bool Prefix(InventoryUI __instance)
        {
            if (!Active) return true;
            var inventory = InventoryManager.Instance;
            if (inventory == null) return false;

            // Keep infinity out of Unity's slider math. No weight-pressure tutorial
            // is needed while unlimited; the original UI resumes outside dungeons.
            if (__instance.weightSlider != null)
            {
                __instance.weightSlider.minValue = 0f;
                __instance.weightSlider.maxValue = 1f;
                __instance.weightSlider.value = 0f;
            }
            if (__instance.weightText != null)
                __instance.weightText.text = $"{inventory.CurrentWeight:0.0} / Unlimited";
            return false;
        }
    }
}
