using DarkRift;
using UnityEngine;

namespace Core.Network
{
    public class NetworkManager : MonoBehaviour
    {
        public string IP = "127.0.0.1";
        public int Port = 4296;

        void Start()
        {
            DarkRiftAPI.workInBackground = true;
            DarkRiftAPI.Connect(IP, Port);
        }

        private void OnApplicationQuit()
        {
            DarkRiftAPI.Disconnect();
        }
    }
}