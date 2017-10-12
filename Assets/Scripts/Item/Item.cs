using System.Xml.Serialization;

[XmlRoot]
public sealed class Item : DatabaseEntry
{
    [XmlElement]
    public string Name { get; set; }

    [XmlElement]
    public DatabaseEntryRef<AssetInfo> PrefabInfoRef { get; set; }

    [XmlElement]
    public string EquipPoint { get; set; }
}