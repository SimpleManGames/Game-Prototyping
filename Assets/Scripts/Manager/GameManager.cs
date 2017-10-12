using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        SceneManager.sceneLoaded += LevelChange;
    }

    private void Start()
    {
        Database.Instance.ReadFiles(Application.dataPath + "/Resources/Database");
        Item item = Database.Instance.GetEntries<Item>().Where(i => i.DatabaseID.ToString() == "TEST_ITEM").FirstOrDefault();
        if (item != null)
           Debug.Log(item.PrefabPath);
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

    private void LevelChange(Scene scene, LoadSceneMode mode)
    {
        _camera = null;
        _camera = FindObjectOfType<Camera>();

        _player = _player ?? FindObjectOfType<Player>();
    }

    public void Quit()
    {
        Application.Quit();
    }
}