using DarkRift;
using UnityEngine;
using static Core.Helper.Hash;

namespace Core.Network.Login
{
    public class LoginManager : MonoBehaviour
    {
        public static int UserID { get; private set; }
        public static bool IsLoggedIn { get; private set; }

        public delegate void SuccessfulLoginEventHandler(int userID);
        public delegate void FailedLoginEventHandler(int reason);
        public delegate void SuccessfulAddUserEventHandler();
        public delegate void FailedAddUserEventHandler();

        public static event SuccessfulLoginEventHandler OnSuccessfulLogin;
        public static event FailedLoginEventHandler OnFailedLogin;
        public static event SuccessfulAddUserEventHandler OnSuccessfulAddUser;
        public static event FailedAddUserEventHandler OnFailedAddUser;

        public static HashType hashType = HashType.SHA256;

        public static void Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return;

            using (DarkRiftWriter writer = new DarkRiftWriter())
            {
                writer.Write(username);
                writer.Write(HashHelper.ReturnHash(password, hashType));
                SendToServer(NT.LoginT, NT.LoginS.loginUser, writer);
            }
        }

        public static void AddUser(string username, string password)
        {
            using (DarkRiftWriter writer = new DarkRiftWriter())
            {
                writer.Write(username);
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

            BindToDataEvent();
        }

        private static void BindToDataEvent()
        {
            if (DarkRiftAPI.isConnected)
            {
                DarkRiftAPI.onData -= OnDataHandler;
                DarkRiftAPI.onData += OnDataHandler;
            }
        }

        private static void OnDataHandler(byte tag, ushort subject, object data)
        {
            if (tag == NT.LoginT)
            {
                if (subject == NT.LoginS.loginUserSuccess)
                {
                    DarkRiftReader reader = (DarkRiftReader)data;

                    UserID = reader.ReadInt32();

                    IsLoggedIn = true;

                    OnSuccessfulLogin?.Invoke(UserID);
                }

                if (subject == NT.LoginS.loginUserFailed)
                {
                    int reason = (int)data;
                    OnFailedLogin?.Invoke(reason);
                }

                if (subject == NT.LoginS.addUserSuccess)
                    OnSuccessfulAddUser?.Invoke();

                if (subject == NT.LoginS.addUserFailed)
                    OnFailedAddUser?.Invoke();
            }
        }
    }
}