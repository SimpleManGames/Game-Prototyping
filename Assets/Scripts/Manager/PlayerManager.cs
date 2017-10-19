using Core.Managers;
using Core.Network.Const;
using Core.Network.Login;
using Core.XmlDatabase;
using DarkRift;
using System.Linq;
using UnityEngine;

namespace Game.Managers
{
    public class PlayerManager : MonoBehaviour
    {
        public delegate void PlayerSavedOKEventHandler();
        public delegate void PlayerLoadOKEventHandler(int id, string playerName, string data);

        public static event PlayerSavedOKEventHandler OnPlayerSavedOK;
        public static event PlayerLoadOKEventHandler OnPlayerLoadOK;

        [ReadOnly] public Player clientPlayer;
        
        private void Start()
        {
            clientPlayer = GameObject.Find("Player").GetComponent<Player>();

            DarkRiftAPI.onDataDetailed += ReceiveData;
            DarkRiftAPI.onPlayerDisconnected += PlayerDisconnected;

            LoadPlayerFromServer.OnCharacterReady += SendNewPlayer;       
        }

        private void SendNewPlayer()
        {
            if(DarkRiftAPI.isConnected)
            {
                DarkRiftAPI.SendMessageToOthers(NT.StartT, NT.StartS.JoinGame, "");

                using (DarkRiftWriter writer = new DarkRiftWriter())
                {
                    //if (clientPlayer == null)
                      //  return;

                    writer.Write(clientPlayer.UserID);
                    writer.Write(clientPlayer.PlayerName);
                    writer.Write(clientPlayer.PlayerData);
                    DarkRiftAPI.SendMessageToOthers(NT.StartT, NT.StartS.Spawn, writer);
                }
            }
        }

        private void ReceiveData(ushort sender, byte tag, ushort subject, object data)
        {
            if(tag == NT.StartT)
            {
                if(subject == NT.StartS.JoinGame)
                {
                    using (DarkRiftWriter writer = new DarkRiftWriter())
                    {
                        //if (clientPlayer == null)
                          //  return;

                        writer.Write(clientPlayer.UserID);
                        writer.Write(clientPlayer.PlayerName);
                        writer.Write(clientPlayer.PlayerData);

                        DarkRiftAPI.SendMessageToID(sender, NT.StartT, NT.StartS.Spawn, writer);
                        return;
                    }
                }

                if(subject == NT.StartS.Spawn)
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

        private void BuildOther(ushort sender, int id, string playerName, string data)
        {
            Debug.Log("Creating Other Player");
            GameObject otherPlayerObject = new GameObject("Player ID: " + sender.ToString());
            Player otherPlayerComponent = otherPlayerObject.AddComponent<Player>();
            otherPlayerComponent.CreateThisPlayer(sender, id, playerName, data);
        }

        private void PlayerDisconnected(ushort id)
        {
            Debug.Log("Disconnect: " + id.ToString());
            GameObject obj = GameObject.Find("Player ID: " + id.ToString());
            Destroy(obj, 0.1f);
        }

        public static void SavePlayer(string playername, byte[] bytes)
        {
            using (DarkRiftWriter writer = new DarkRiftWriter())
            {
                writer.Write(LoginManager.UserID);
                writer.Write(playername);
                writer.Write(bytes);
                SendToServer(NT.PlayerT, NT.PlayerS.playerSaveData, writer);
            }
        }

        public static void LoadPlayer()
        {
            using (DarkRiftWriter writer = new DarkRiftWriter())
            {
                writer.Write(LoginManager.UserID);
                SendToServer(NT.PlayerT, NT.PlayerS.playerLoadData, writer);
            }
        }

        private static void OnDataHandler(byte tag, ushort subject, object data)
        {
            if (tag == NT.PlayerT)
            {
                if (subject == NT.PlayerS.playerRecieveData)
                {
                    using (DarkRiftReader reader = (DarkRiftReader)data)
                    {
                        int id = reader.ReadInt32();
                        string playerName = reader.ReadString();
                        string playerData = reader.ReadString();

                        OnPlayerLoadOK?.Invoke(id, playerName, playerData);
                    }
                }

                if (subject == NT.PlayerS.playerSavedOkData)
                {
                    OnPlayerSavedOK?.Invoke();
                }
            }
        }

        private static void SendToServer(byte tag, ushort subject, object data)
        {
            if(DarkRiftAPI.isConnected)
                DarkRiftAPI.SendMessageToServer(tag, subject, data);
            else
                Debug.LogError("[PlayerManager] You can't send a message to the server if not connected.");

            BindToOnDataEvent();
        }

        private static void BindToOnDataEvent()
        {
            if(DarkRiftAPI.isConnected)
            {
                DarkRiftAPI.onData -= OnDataHandler;
                DarkRiftAPI.onData += OnDataHandler;
            }
        }
    }
}