using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public delegate DatabaseEntry DatabaseEntryFactoryDelegate(XmlReader reader);

public class DatabaseEntryFactory
{
    private static Dictionary<string, DatabaseEntryFactoryDelegate> typeMap = CreateTypeMap();

    public static Dictionary<string, DatabaseEntryFactoryDelegate> CreateTypeMap()
    {
        Dictionary<string, DatabaseEntryFactoryDelegate> map = new Dictionary<string, DatabaseEntryFactoryDelegate>();

        Type baseType = typeof(DatabaseEntry);

        Assembly currentAssembly = Assembly.GetExecutingAssembly();

        foreach (Type type in currentAssembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract || !type.IsSubclassOf(baseType))
                continue;

            Type tempType = type;
            map.Add(type.ToString(), (reader) =>
            {
                XmlSerializer serializer = new XmlSerializer(tempType);
                return (DatabaseEntry)serializer.Deserialize(reader);
            });
        }

        return map;
    }

    public static DatabaseEntry Create(string entryTypeName, XmlReader reader)
    {
        if (entryTypeName == null)
            return null;

        DatabaseEntryFactoryDelegate del;
        if (typeMap.TryGetValue(entryTypeName, out del))
            return del(reader);

        Debug.LogError("Unknown Database Entry type: " + entryTypeName);
        return null;
    }
}