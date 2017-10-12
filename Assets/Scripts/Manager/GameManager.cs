using System;
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

    [SerializeField, ReadOnly]
    private ResourceManager _resourceManager;
    public ResourceManager ResourceManager
    {
        get { return _resourceManager; }
    }

    public event Action OnFinishedLoading;

    public override void Awake()
    {
        base.Awake();
        _player = FindObjectOfType<Player>();
        _camera = FindObjectOfType<Camera>();
        _resourceManager = GetComponent<ResourceManager>();
        _stateMachine = _stateMachine ?? new StateMachine();

        OnFinishedLoading += () => Debug.Log("Finished");

        Debug.Log(Application.streamingAssetsPath);
        Database.Instance.ReadFiles(Application.streamingAssetsPath + "/XML/");
        ResourceManager.LoadBundlesAsync(() => OnFinishedLoading?.Invoke());

        SceneManager.sceneLoaded += LevelChange;
    }

    private void Start()
    {
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetPlayer();

        if (Input.GetKeyDown(KeyCode.I))
        {
            Item item = Database.Instance.GetEntries<Item>().Where(i => i.DatabaseID.ToString() == "VIKING_SWORD").FirstOrDefault();
            ResourceManager.LoadAssetAsync<GameObject>(item.PrefabInfoRef.Entry, (prefab) =>
            {
                Instantiate(prefab);
            });
        }

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