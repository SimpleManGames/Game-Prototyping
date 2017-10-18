using System;
using Core.XmlDatabase;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Managers;

namespace Core.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField]
        private StateMachine _stateMachine;
        public StateMachine StateMachine
        {
            get { return _stateMachine; }
        }

        [SerializeField, ReadOnly]
        private PlayerManager _playerManager;
        public PlayerManager PlayerManager
        {
            get { return _playerManager; }
            set { _playerManager = value; }
        }


        // When multiplayer is more implemented these need to go

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

        // -----------------------------------------------------

        [SerializeField, ReadOnly]
        private ResourceManager _resourceManager;
        public ResourceManager ResourceManager
        {
            get { return _resourceManager; }
        }

        [SerializeField]
        private GameObject _pauseUI;
        public GameObject PauseUI { get { return _pauseUI; } }
        
        public event Action OnFinishedLoading;

        public override void Awake()
        {
            base.Awake();
            _player = FindObjectOfType<Player>();
            _camera = FindObjectOfType<Camera>();
            _resourceManager = GetComponent<ResourceManager>();
            _stateMachine = _stateMachine ?? new StateMachine();
            //_playerManager = FindObjectOfType<PlayerManager>(); //GetComponent<PlayerManager>();

            Database.Instance.ReadFiles(Application.streamingAssetsPath + "/XML/");
            ResourceManager.LoadBundlesAsync(() => OnFinishedLoading?.Invoke());

            SceneManager.sceneLoaded += LevelChange;
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

        public void LoadScene(string scene)
        {
            SceneManager.LoadScene(scene);
        }

        private void LevelChange(Scene scene, LoadSceneMode mode)
        {
            _camera = null;
            _camera = FindObjectOfType<Camera>();

            _player = _player ?? GameObject.Find("Player")?.GetComponent<Player>();

            if(scene.name == "controllerDev")
                _playerManager = FindObjectOfType<PlayerManager>();
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}