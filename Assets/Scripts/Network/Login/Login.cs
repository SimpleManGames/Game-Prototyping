#if UNITY_EDITOR
using UnityEditor;
#endif
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
            LoginManager.OnFailedLogin += LoginFailed;
            LoginManager.OnSuccessfulAddUser += ButtonLogin;
            LoginManager.OnFailedAddUser += ButtonQuit;
        }

        private void OnApplicationQuit()
        {
            LoginManager.OnSuccessfulLogin -= ChangeToFirstLevel;
            LoginManager.OnFailedLogin -= LoginFailed;
            LoginManager.OnSuccessfulAddUser -= ButtonLogin;
            LoginManager.OnFailedAddUser -= ButtonQuit;
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

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        private void ChangeToFirstLevel()
        {
            SceneManager.LoadScene("Menu");
        }

        private void LoginFailed(int reason)
        {
            if (reason == 0)
                passwordInput.text = "";
            else
                ButtonQuit();
        }
    }
}