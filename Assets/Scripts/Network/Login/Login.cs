using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.Network.Login
{
    public class Login : MonoBehaviour
    {
        public InputField userNameInput;
        public InputField passwordInput;

        public void Start()
        {
            LoginManager.OnSuccessfulLogin += ChangeToFirstLevel;
        }

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

        private void ChangeToFirstLevel(int userID, bool hasChar)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}