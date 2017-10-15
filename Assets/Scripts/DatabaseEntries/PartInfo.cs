using Core.XmlDatabase;
using System.Xml.Serialization;

public enum PartType
{
    [XmlEnum] Head,
    [XmlEnum] Core,
    [XmlEnum] Legs,
    [XmlEnum] Arms,
    [XmlEnum] Booster
}

[XmlRoot]
public class PartInfo : DatabaseEntry
{
    [XmlElement]
    public string Name { get; set; }
    
    [XmlElement]
    public DatabaseEntryRef<AssetInfo> Prefab { get; set; }

    [XmlElement]
    public PartType Type { get; set; }
}