using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Linq;

internal class DatabaseTable : Dictionary<ID, DatabaseEntry> { }

public sealed class Database
{
    private static Database _instance;
    public static Database Instance
    {
        get
        {
            return _instance = _instance ?? new Database();
        }
    }

    internal event Action OnFinishedLoading;
    private Dictionary<Type, DatabaseTable> tables = new Dictionary<Type, DatabaseTable>();

    public void ReadFiles(string rootPath)
    {
        Clear();

        Stack<string> dirs = new Stack<string>();

        dirs.Push(rootPath);
        while (dirs.Count > 0)
        {
            string currentDir = dirs.Pop();
            string[] subDirs = Directory.GetDirectories(currentDir);
            string[] files = Directory.GetFiles(currentDir);

            foreach (string file in files)
                if (Path.GetExtension(file).ToLower() == ".xml")
                    ReadDatabaseFile("file://" + file);

            if (subDirs != null && subDirs.Length > 0)
                foreach (string dir in subDirs)
                    dirs.Push(dir);
        }

        OnFinishedLoading?.Invoke();

        PostLoad();
    }

    private void Clear()
    {
        tables = new Dictionary<Type, DatabaseTable>();
    }

    private void ReadDatabaseFile(string path)
    {
        using (XmlReader reader = XmlReader.Create(path))
        {
            while (reader.Read() && reader.MoveToContent() == XmlNodeType.Element)
            {
                if (reader.Name != "Database")
                {
                    DatabaseEntry entry = DatabaseEntryFactory.Create(reader.Name, reader);
                    if (entry != null)
                        AddEntry(entry);
                }
            }
        }
    }

    private void PostLoad()
    {
        foreach (DatabaseTable table in tables.Values)
            foreach (DatabaseEntry entry in table.Values)
                entry.PostLoad(this);
    }

    private void AddEntry(DatabaseEntry entry)
    {
        DatabaseTable table;

        if (!tables.TryGetValue(entry.GetType(), out table))
        {
            Debug.Log("Created table for: " + entry.GetType().ToString());

            table = new DatabaseTable();
            
            tables.Add(entry.GetType(), table);
        }

        if (ID.IsNullOrNoID(entry.DatabaseID))
        {
            Debug.LogError("Database Entry has no ID: " + entry.GetType().ToString());
            return;
        }

        if (!table.ContainsKey(entry.DatabaseID))
            table.Add(entry.DatabaseID, entry);
        else
            Debug.LogWarning("Duplicate database entry: " + entry.DatabaseID);
    }

    public T GetEntry<T>(ID id) where T : DatabaseEntry
    {
        DatabaseTable table;

        if (tables.TryGetValue(typeof(T), out table))
        {
            DatabaseEntry result;

            if (table.TryGetValue(id, out result))
                return (T)result;
        }

        return null;
    }

    public T GetEntry<T>(string idString) where T : DatabaseEntry
    {
        return GetEntry<T>(ID.GetID(idString));
    }

    public T[] GetEntries<T>() where T : DatabaseEntry
    {
        DatabaseTable table;

        if (tables.TryGetValue(typeof(T), out table))
            return Enumerable.Cast<T>(table.Values).ToArray();

        return null;
    }
}
