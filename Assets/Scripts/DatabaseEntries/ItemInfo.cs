using System.Xml.Serialization;

public enum ItemType
{
    [XmlEnum] Weapon,
    [XmlEnum] Armor,
    [XmlEnum] Usable
}

[XmlRoot]
public sealed class ItemInfo : DatabaseEntry
{
    [XmlElement]
    public string Name { get; set; }

    [XmlElement]
    public DatabaseEntryRef<AssetInfo> Prefab { get; set; }

    [XmlElement]
    public DatabaseEntryRef<AssetInfo> Icon { get; set; }

    [XmlElement]
    public ItemType Type { get; set; } 
}