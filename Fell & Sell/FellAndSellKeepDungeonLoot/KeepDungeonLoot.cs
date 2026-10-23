using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppGame;
using MelonLoader;

[assembly: MelonInfo(typeof(FellAndSell.KeepDungeonLoot), "Keep Dungeon Loot", "1.0.0", "Local Mod")]
[assembly: MelonGame("Art Games Studio SA", "Fell & Sell")]
[assembly: MelonGame("Art Games Studio SA", "Fell _ Sell")]

namespace FellAndSell;

public sealed class KeepDungeonLoot : MelonMod
{
    private static MelonPreferences_Entry<bool> enabled;
    [ThreadStatic] private static bool preservingLoot;

    public override void OnInitializeMelon()
    {
        enabled = MelonPreferences.CreateCategory("KeepDungeonLoot")
            .CreateEntry("Enabled", true, description: "Keep inventory, equipment, and gold when dying in a dungeon or the forest.");
        LoggerInstance.Msg("Ready. Dungeon and forest deaths retain carried items, equipment, quick slots, and gold. Death and return to town still occur.");
    }

    [HarmonyPatch]
    private static class DeathRoutineScope
    {
        private static MethodBase TargetMethod()
        {
            // IL2CPP's coroutine factory may be inlined. Hook the native iterator step,
            // resolving its generated name without depending on the compiler's numeric suffix.
            var routine = typeof(PlayerStats).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .Single(type => type.Name.StartsWith("_DeathRoutine_d__", StringComparison.Ordinal));
            return AccessTools.Method(routine, "MoveNext");
        }

        private static void Prefix(out bool __state)
        {
            __state = preservingLoot;
            preservingLoot = enabled?.Value == true && (GameSceneManager.IsDungeon || GameSceneManager.IsForest);
        }

        // Each MoveNext returns at a yield. Never leave protection active between frames,
        // after an exception, or during an unrelated inventory reset/new game.
        private static void Finalizer(bool __state) => preservingLoot = __state;
    }

    [HarmonyPatch]
    private static class DeathLosses
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(InventoryManager), nameof(InventoryManager.ClearAllItems));
            yield return AccessTools.Method(typeof(PlayerEquipment), nameof(PlayerEquipment.DropAllEquippedVisuals));
            yield return AccessTools.Method(typeof(ShopManager), nameof(ShopManager.ResetGold));
        }

        // Keep the original objects and their metadata. No restore/copy means no duplicate
        // loot, and the death routine's normal player-state save persists the retained stock.
        private static bool Prefix() => !preservingLoot;
    }
}
