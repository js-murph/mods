using System.Runtime.CompilerServices;

// Models the inspected weight comparisons, not native Unity/IL2CPP execution.
namespace Il2Cpp
{
    public record Item(float Weight, int Quantity, int Upgrade = 0, Item[] Contents = null)
    {
        public float TotalWeight => Weight * Quantity + (Contents?.Sum(item => item.TotalWeight) ?? 0f);
    }

    public class InventoryManager
    {
        public static InventoryManager Instance;
        public float maxWeight = 20f, EquipmentBonus = 7f, DungeonBonus = 3f;
        public List<Item> Items = new();
        public float CurrentWeight => Items.Sum(item => item.TotalWeight);
        public float EffectiveMaxWeight
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get => maxWeight + EquipmentBonus + DungeonBonus;
        }

        public bool CanCarryItem(Item item) => item != null && item.Quantity > 0 &&
            CurrentWeight + item.TotalWeight <= EffectiveMaxWeight;

        public bool AddItem(Item item)
        {
            // AddItem has its own capacity check; it does not call CanCarryItem.
            if (item == null || item.Quantity <= 0) return false;
            if (CurrentWeight + item.TotalWeight > EffectiveMaxWeight) return false;
            Items.Add(item);
            return true;
        }
    }

    public class InventoryUI
    {
        public Slider weightSlider = new();
        public Label weightText = new();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void UpdateWeightUI()
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null) return;
            if (weightSlider != null)
            {
                weightSlider.maxValue = inventory.EffectiveMaxWeight;
                weightSlider.value = Math.Clamp(inventory.CurrentWeight, weightSlider.minValue, weightSlider.maxValue);
            }
            if (weightText != null)
                weightText.text = $"{inventory.CurrentWeight:0.0} / {inventory.EffectiveMaxWeight:0.0}";
        }
    }

    public class Slider { public float minValue, maxValue = 1f, value; }
    public class Label { public string text; }
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
