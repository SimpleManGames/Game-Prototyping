using UnityEngine;
using UnityEngine.UI;

namespace Core.Network.Login
{
    public class Login : MonoBehaviour
    {
        public InputField userNameInput;
        public InputField passwordInput;

        public void ButtonLogin()
        {
            LoginManager.Login(userNameInput.text, passwordInput.text);
        }

        public void ButtonAddUser()
        {
            LoginManager.AddUser(userNameInput.text, passwordInput.text);
        }

        public void ButtonQuit()
        {
            Application.Quit();
        }
    }
}