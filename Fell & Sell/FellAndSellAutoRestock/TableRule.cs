namespace FellAndSell;

public enum RestockMode { Off, Always, DemandOnly }

// Plain JSON data stored alongside the game's own shelf data, not Unity object references.
public sealed class TableRule
{
    public RestockMode Mode { get; set; }
    public string ItemName { get; set; }
    public int BasePrice { get; set; }
    public string Scene { get; set; }
    public string SlotName { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}
