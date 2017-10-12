using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("AssetInfo")]
public sealed class AssetInfo : DatabaseEntry
{
    [XmlElement]
    public string Path { get; set; }

    [XmlElement]
    public DatabaseEntryRef<AssetBundleInfo> AssetBundleInfoRef { get; set; }
}