using System.Xml.Serialization;

[XmlRoot]
public class AnimationInfo : DatabaseEntry
{
    [XmlElement]
    public string AnimationParamName { get; set; }
}