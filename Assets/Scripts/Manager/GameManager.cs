using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private StateMachine _stateMachine;
    public StateMachine StateMachine
    {
        get { return _stateMachine; }
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

        _stateMachine = _stateMachine ?? new StateMachine();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetPlayer();

        _stateMachine?.CurrentState?.Update();
    }

    public void ResetPlayer()
    {
        Player.transform.position = Vector3.zero;
        Player.moveDirection = Vector3.zero;
    }
}