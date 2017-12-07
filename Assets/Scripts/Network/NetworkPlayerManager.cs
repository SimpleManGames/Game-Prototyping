using Core.Network.Const;
using DarkRift;
using System;
using UnityEngine;

public class NetworkPlayerManager : MonoBehaviour
{
    private Player clientPlayer;
    
    private void Start()
    {
        clientPlayer = GameObject.Find("Player").GetComponent<Player>();

        DarkRiftAPI.onDataDetailed += ReceiveData;
        DarkRiftAPI.onPlayerDisconnected += PlayerDisconnected;
        Player.OnPlayerReady += SendNewPlayer;
    }

    //
    // Summary:
    //      Sends the client's player info to other clients 
    //      so they can create their instances of this client
    //
    private void SendNewPlayer()
    {
        if (DarkRiftAPI.isConnected)
        {
            DarkRiftAPI.SendMessageToOthers(NT.StartT, NT.StartS.JoinGame, "");

            using (DarkRiftWriter writer = new DarkRiftWriter())
            {
                writer.Write(clientPlayer.UserID);
                writer.Write(clientPlayer.PlayerName);
                writer.Write(clientPlayer.PlayerData);
                DarkRiftAPI.SendMessageToOthers(NT.StartT, NT.StartS.Spawn, writer);
            }
        }
    }

    private void PlayerDisconnected(ushort id)
    {
        GameObject obj = GameObject.Find(id.ToString());
        Debug.Log(id.ToString());
        Destroy(obj, 0.1f);
    }

    private void ReceiveData(ushort sender, byte tag, ushort subject, object data)
    {
        if (tag == NT.StartT)
        {
            if (subject == NT.StartS.JoinGame)
            {
                using (DarkRiftWriter writer = new DarkRiftWriter())
                {
                    writer.Write(clientPlayer.UserID);
                    writer.Write(clientPlayer.PlayerName);
                    writer.Write(clientPlayer.PlayerData);

                    DarkRiftAPI.SendMessageToID(sender, NT.StartT, NT.StartS.Spawn, writer);
                    return;
                }
            }

            if (subject == NT.StartS.Spawn)
            {
                using (DarkRiftReader reader = (DarkRiftReader)data)
                {
                    int id = reader.ReadInt32();
                    string playerName = reader.ReadString();
                    string playerData = reader.ReadString();

                    BuildOther(sender, id, playerName, playerData);
                    return;
                }
            }
        }
    }

    //
    // Summary:
    //
    //      Creates the object representing another player
    //
    private void BuildOther(ushort sender, int id, string playerName, string data)
    {
        GameObject otherPlayerObject = new GameObject(sender.ToString());
        NetworkPlayer networkPlayer = otherPlayerObject.AddComponent<NetworkPlayer>();
        networkPlayer.MakePlayer(sender, id, playerName, data);
    }
}