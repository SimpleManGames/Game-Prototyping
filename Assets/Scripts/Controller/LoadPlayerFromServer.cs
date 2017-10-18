using Core.Managers;
using Core.Network;
using Core.Network.Login;
using Core.XmlDatabase;
using DarkRift;
using Game.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadPlayerFromServer : MonoBehaviour
{
    private PartInfo[] parts;

    private void Awake()
    {
        parts = Database.Instance.GetEntries<PartInfo>();
    }

    void Start()
    {
        if (DarkRiftAPI.isConnected)
        {
            PlayerManager.OnPlayerLoadOK += LoadPlayer;
            PlayerManager.LoadPlayer();
        }
    }

    private void LoadPlayer(int id, string playerName, string data)
    {
        if (LoginManager.UserID != id)
            return;

        int count = 6;
        int k = 0;

        List<string> partStrings = data.ToLookup(c => Mathf.Floor(k++ / count)).Select(e => new String(e.ToArray())).ToList();

        foreach (string partName in partStrings)
        {
            foreach (PartInfo part in parts.Where(p => p.Name == partName))
            {
                GameManager.Instance.ResourceManager.LoadAssetAsync<GameObject>(part.Prefab.Entry, (prefab) => { Instantiate(prefab, transform.GetChild(0).transform); });
            }
        }

        PlayerManager.OnPlayerLoadOK -= LoadPlayer;
    }
}
