using System.Xml.Serialization;

[XmlRoot]
public sealed class Item : DatabaseEntry
{
    [XmlElement]
    public string Name { get; set; }

    [XmlElement]
    public string PrefabPath { get; set; }

    [XmlElement]
    public string IconPath { get; set; }
}