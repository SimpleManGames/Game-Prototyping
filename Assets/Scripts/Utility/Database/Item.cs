using System.Xml.Serialization;

[XmlRoot]
public class Item : DatabaseEntry
{
    [XmlElement]
    public string ModelPath { get; private set; }

    [XmlElement]
    public string IconPath { get; private set; }
}