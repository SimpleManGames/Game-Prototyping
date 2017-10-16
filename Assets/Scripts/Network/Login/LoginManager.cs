using UnityEngine;
using DarkRift;

namespace Core.Network.Login
{
    public class LoginManager : MonoBehaviour
    {
        public static void Login(string userName, string password)
        {
            using (DarkRiftWriter writer = new DarkRiftWriter())
            {
                writer.Write(userName);
                writer.Write(password);
                SendToServer(NT.LoginT, NT.LoginS.loginUser, writer);
            }
        }

        public static void AddUser(string userName, string password)
        {
            using (DarkRiftWriter writer = new DarkRiftWriter())
            {
                writer.Write(userName);
                writer.Write(password);
                SendToServer(NT.LoginT, NT.LoginS.addUser, writer);
            }
        }

        private static void SendToServer(byte tag, ushort subject, object data)
        {
            if (DarkRiftAPI.isConnected)
                DarkRiftAPI.SendMessageToServer(tag, subject, data);
            else
                Debug.LogError("[Login]: You can't add a user if you're not connected to a server");
        }
    }
}