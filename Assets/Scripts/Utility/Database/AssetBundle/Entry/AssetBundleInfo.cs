using Core.XmlDatabase;
using System.Xml.Serialization;

[XmlRoot("AssetBundleInfo")]
public sealed class AssetBundleInfo : DatabaseEntry
{
    [XmlElement]
    public string Name { get; set; }

    [XmlElement]
    public string URL { get; set; }
}
