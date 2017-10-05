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

    [SerializeField, ReadOnly]
    private Camera _camera;
    public Camera Camera
    {
        get { return _camera; }
    }

    public override void Awake()
    {
        base.Awake();
        _player = FindObjectOfType<Player>();
        _camera = FindObjectOfType<Camera>();
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