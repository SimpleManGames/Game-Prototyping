using Core.Network.Const;
using Core.Network.Login;
using DarkRift;
using UnityEngine;

namespace Game.Managers
{
    public class PlayerManager : MonoBehaviour
    {
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

        public void LoadPlayer()
        {

        }

        private static void OnDataHandler(byte tag, ushort subject, object data)
        {
            if (tag == NT.PlayerT)
            {

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