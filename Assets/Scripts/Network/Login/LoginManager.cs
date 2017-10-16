using UnityEngine;
using DarkRift;
using static Core.Helper.Hash;

namespace Core.Network.Login
{
    public class LoginManager : MonoBehaviour
    {
        public static HashType hashType = HashType.SHA256;

        public static void Login(string userName, string password)
        {
            using (DarkRiftWriter writer = new DarkRiftWriter())
            {
                writer.Write(userName);
                writer.Write(HashHelper.ReturnHash(password, hashType));
                SendToServer(NT.LoginT, NT.LoginS.loginUser, writer);
            }
        }

        public static void AddUser(string userName, string password)
        {
            using (DarkRiftWriter writer = new DarkRiftWriter())
            {
                writer.Write(userName);
                writer.Write(HashHelper.ReturnHash(password, hashType));
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