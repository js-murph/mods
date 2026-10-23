using System.Runtime.CompilerServices;
using System.Text.Json;

// Models the inspected death sequence, not Unity or IL2CPP execution. No game data/files.
namespace Il2Cpp
{
    public record Item(string Name, int Quantity, int Upgrade, Item[] Contents);

    public class InventoryManager
    {
        public static InventoryManager Instance;
        public List<Item> Backpack = new(), Equipped = new(), Quick = new();
        public string Snapshot() => JsonSerializer.Serialize(new { Backpack, Equipped, Quick });

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ClearAllItems(bool keepQuestItems, bool keepEquipped)
        {
            Backpack.Clear(); Quick.Clear();
            if (!keepEquipped) Equipped.Clear();
        }
    }

    public class PlayerEquipment
    {
        public static PlayerEquipment Instance;
        public bool HasVisuals = true;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DropAllEquippedVisuals() => HasVisuals = false;
    }

    public class ShopManager
    {
        public static ShopManager Instance;
        public int Gold = 137;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ResetGold() => Gold = 0;
    }

    public class PlayerStats
    {
        public bool KeepEquippedOnDeath, ResetGoldOnDeath = true, ThrowAfterLosses;
        public bool PlayedDeathAnimation, ReturnedToTown;
        public string SavedInventory;
        public int SavedGold;
        public _DeathRoutine_d__999 DeathRoutine() => new(this);

        // Deliberately different suffix from the game to test version-independent resolution.
        public class _DeathRoutine_d__999
        {
            private readonly PlayerStats player;
            private int step;
            public _DeathRoutine_d__999(PlayerStats player) => this.player = player;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public bool MoveNext()
            {
                if (step++ == 0)
                {
                    if (!player.KeepEquippedOnDeath) PlayerEquipment.Instance.DropAllEquippedVisuals();
                    InventoryManager.Instance.ClearAllItems(true, player.KeepEquippedOnDeath);
                    if (player.ResetGoldOnDeath) ShopManager.Instance.ResetGold();
                    if (player.ThrowAfterLosses) throw new InvalidOperationException("Simulated native failure");
                    player.PlayedDeathAnimation = true;
                    return true;
                }
                player.SavedInventory = InventoryManager.Instance.Snapshot();
                player.SavedGold = ShopManager.Instance.Gold;
                player.ReturnedToTown = true;
                Il2CppGame.GameSceneManager.IsDungeon = false;
                Il2CppGame.GameSceneManager.IsForest = false;
                return false;
            }
        }
    }
}

namespace Il2CppGame
{
    public static class GameSceneManager { public static bool IsDungeon, IsForest; }
}

namespace MelonLoader
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class MelonInfoAttribute : Attribute { public MelonInfoAttribute(Type type, string name, string version, string author) { } }
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class MelonGameAttribute : Attribute { public MelonGameAttribute(string developer, string name) { } }
    public class MelonMod { public Logger LoggerInstance = new(); public virtual void OnInitializeMelon() { } }
    public class Logger { public void Msg(string message) { } }
    public class MelonPreferences_Entry<T> { public T Value; }
    public static class MelonPreferences
    {
        public static MelonPreferences_Entry<bool> Enabled;
        public static Category CreateCategory(string name) => new();
        public class Category
        {
            public MelonPreferences_Entry<bool> CreateEntry(string name, bool value, string description) =>
                Enabled = new() { Value = value };
        }
    }
}
