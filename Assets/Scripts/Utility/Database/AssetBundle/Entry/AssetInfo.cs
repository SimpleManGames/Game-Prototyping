using System.Xml.Serialization;

[XmlRoot("AssetInfo")]
public sealed class AssetInfo : DatabaseEntry
{
    [XmlElement]
    public string Path { get; set; }

    [XmlElement]
    public DatabaseEntryRef<AssetBundleInfo> AssetBundleInfoRef { get; set; }
}