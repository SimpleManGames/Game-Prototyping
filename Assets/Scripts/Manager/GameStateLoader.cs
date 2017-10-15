using Core.Managers;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateLoader : MonoBehaviour
{
    [SerializeField]
    private string stateScriptName;

    private void Awake()
    {
        if (GameManager.Instance == null)
            SceneManager.LoadScene("GameManager", LoadSceneMode.Additive);
    }

    private void Start()
    {
        if (String.IsNullOrEmpty(stateScriptName))
            return;

        Type[] result = AppDomain.CurrentDomain.GetAllDerivedTypes_IsAssignableFrom(typeof(IState));
        
        foreach (Type type in result)
        {
            if (type.ToString() == stateScriptName)
            {
                GameManager.Instance.StateMachine.CurrentState = (IState)Activator.CreateInstance(type);
                return;
            }
        }
        
    }

    public void LoadScene(string scene)
    {
        GameManager.Instance.LoadScene(scene);
    }

    public void Quit()
    {
        GameManager.Instance.Quit();
    }
}