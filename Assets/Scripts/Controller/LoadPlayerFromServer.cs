using Core.Network.Login;
using DarkRift;
using Game.Managers;
using UnityEngine;

public class LoadPlayerFromServer : MonoBehaviour
{
    public delegate void CharacterReadyEventHandler();

    public static event CharacterReadyEventHandler OnCharacterReady;

    void Start()
    {
        if (DarkRiftAPI.isConnected)
        {
            PlayerManager.OnPlayerLoadOK += LoadPlayer;
            PlayerManager.LoadPlayer();
        }
    }

    private void LoadPlayer(int id, string playerName, string data)
    {
        if (LoginManager.UserID != id)
            return;

        GetComponent<Player>().CreateThisPlayer(id, playerName, data);

        PlayerManager.OnPlayerLoadOK -= LoadPlayer;
        OnCharacterReady?.Invoke();
    }
}
