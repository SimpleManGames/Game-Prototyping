using System;
using UnityEngine;

public class GameStateLoader : MonoBehaviour
{
    [SerializeField]
    public string stateScriptName;

    void Start()
    {
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

    void Update() { }
}