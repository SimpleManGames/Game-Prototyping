using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("AssetBundleInfo")]
public sealed class AssetBundleInfo : DatabaseEntry
{
    [XmlElement]
    public string Name { get; set; }

    [XmlElement]
    public string URL { get; set; }
}