using Core.Managers;
using Core.Network.Const;
using Core.XmlDatabase;
using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    #region _Network Info_

    [Header("Network Info")]
    [SerializeField, ReadOnly]
    private int _networkID;
    public int NetworkID
    {
        get { return _networkID; }
        private set { _networkID = value; }
    }

    [SerializeField, ReadOnly]
    private int _userID;
    public int UserID
    {
        get { return _userID; }
        private set { _userID = value; }
    }

    [SerializeField, ReadOnly]
    private string _playerName;
    public string PlayerName
    {
        get { return _playerName; }
        private set { _playerName = value; }
    }

    [SerializeField, ReadOnly]
    private string _playerData;
    public string PlayerData
    {
        get { return _playerData; }
        private set { _playerData = value; }
    }

    [SerializeField, ReadOnly]
    private Vector3 _networkPosition;
    [SerializeField, ReadOnly]
    private Quaternion _networkRotation;


    #endregion

    private void Start()
    {
        DarkRiftAPI.onDataDetailed += RecieveDetailedData;
    }

    private void OnApplicationQuit()
    {
        DarkRiftAPI.onDataDetailed -= RecieveDetailedData;
    }

    private void RecieveDetailedData(ushort sender, byte tag, ushort subject, object data)
    {
        if (sender == _networkID)
        {
            if (tag == NT.MoveT)
            {
                if (subject == NT.MoveS.Position)
                    transform.position = (Vector3)data;

                if (subject == NT.MoveS.Rotation)
                    transform.rotation = (Quaternion)data;
            }
        }
    }

    //
    // Summary:
    //      Makes the client player
    //
    internal void MakePlayer(ushort sender, int id, string playerName, string data)
    {
        NetworkID = sender;
        UserID = id;
        PlayerName = playerName;
        PlayerData = data;

        CreatePlayer(data);
    }

    //
    // Summary:
    //      Creates the player for this reference of Player
    //
    private void CreatePlayer(string data)
    {
        int count = 6;
        int k = 0;

        List<string> partStrings = data.ToLookup(c => Mathf.Floor(k++ / count)).Select(e => new String(e.ToArray())).ToList();

        foreach (string partName in partStrings)
            foreach (PartInfo part in Database.Instance.GetEntries<PartInfo>().Where(p => p.Name == partName))
                GameManager.Instance.ResourceManager.LoadAssetAsync<GameObject>(part.Prefab.Entry, (prefab) =>
                {
                    Transform child = transform.Find("Model");
                    if (child == null)
                    {
                        GameObject newObj = new GameObject("Model");
                        newObj.transform.parent = transform;
                        child = newObj.transform;
                    }

                    Instantiate(prefab, child);
                });
    }
}
