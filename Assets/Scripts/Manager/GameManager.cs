using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField, ReadOnly]
    private GameStateEnum _currentState;
    public GameStateEnum CurrentState
    {
        get
        {
            return _currentState;
        }
    }

    [SerializeField, ReadOnly]
    private Player _player;
    public Player Player
    {
        get { return _player; }
    }

    public override void Awake()
    {
        base.Awake();
        _player = FindObjectOfType<Player>();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Player.transform.position = Vector3.zero;
            Player.moveDirection = Vector3.zero;
        }
    }
}