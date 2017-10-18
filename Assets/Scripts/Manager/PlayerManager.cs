using Core.Network.Const;
using Core.Network.Login;
using DarkRift;
using UnityEngine;

namespace Game.Managers
{
    public class PlayerManager
    {
        public delegate void PlayerSavedOKEventHandler();
        public delegate void PlayerLoadOKEventHandler(int id, string playerName, string data);

        public static event PlayerSavedOKEventHandler OnPlayerSavedOK;
        public static event PlayerLoadOKEventHandler OnPlayerLoadOK;

        public static void SavePlayer(string playername, byte[] bytes)
        {
            using (DarkRiftWriter writer = new DarkRiftWriter())
            {
                writer.Write(LoginManager.UserID);
                writer.Write(playername);
                writer.Write(bytes);
                Debug.Log(bytes.ToString());
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