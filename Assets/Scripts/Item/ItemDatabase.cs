using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Item System/Database")]
public class ItemDatabase : ScriptableObject
{
    /// <summary>
    /// Keeping the instance for now since it's a good way to make sure you only have one database
    /// </summary>
    static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get { return _instance; }
    }

    [SerializeField]
    public List<Item> items = new List<Item>();

    private void OnEnable()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Debug.LogError("Item Database Instance already exists in the scope; Deleting newly created Database Object", this);
            DestroyImmediate(this);
            return;
        }


    }
}