using Core.Managers;
using Core.Network.Const;
using Core.XmlDatabase;
using DarkRift;
using Game.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SelectionBase, RequireComponent(typeof(Controller)), RequireComponent(typeof(PlayerInputController))]
public class Player : Agent
{
    [HideInInspector]
    public PlayerInputController input;

    [HideInInspector]
    public Transform cameraRigTransform;
    private CameraController cameraController;

    Controller controller;
    public Animator Animator { get; private set; }
    private GameObject modelObject;

    #region _Movement Info_

    [Header("Movement Info")]
    [ReadOnly, Tooltip("Describes the direction the player controller is trying to move. " +
        "If you want to manually move this, use the Controller's debug move")]
    public Vector3 moveDirection;
    [SerializeField, ReadOnly]
    private float moveAmount;
    public float MoveAmount { get; set; }
    [ReadOnly] public bool canMove;

    #endregion

    #region _Network Info_

    [SerializeField, ReadOnly]
    private int _networkID;
    public int NetworkID
    {
        get { return _networkID; }
        private set { _networkID = value; }
    }

    [SerializeField, ReadOnly]
    private int _userID;
    public int UserID
    {
        get { return _userID; }
        private set { _userID = value; }
    }

    [SerializeField, ReadOnly]
    private string _playerName;
    public string PlayerName
    {
        get { return _playerName; }
        private set { _playerName = value; }
    }

    [SerializeField, ReadOnly]
    private string _playerData;
    public string PlayerData
    {
        get { return _playerData; }
        private set { _playerData = value; }
    }

    public bool IsClientPlayer { get { return name == "Player"; } }

    [SerializeField, ReadOnly]
    private Vector3 _networkPosition;
    [SerializeField, ReadOnly]
    private Quaternion _networkRotation;

    #endregion

    public delegate void CharacterReadyEventHandler();
    public static event CharacterReadyEventHandler OnCharacterReady;

    #region _Update API_

    public override void Awake()
    {
        if (GameManager.Instance.PlayerManager == null)
            GameManager.Instance.PlayerManager = FindObjectOfType<PlayerManager>();

        //GameManager.Instance.PlayerManager.clientPlayer = this;

        base.Awake();
        controller = GetComponent<Controller>();
        Animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInputController>();
        state.CurrentState = new PlayerIdleState(state, this, controller);

        modelObject = transform.Find("Model")?.gameObject;

        if (!IsClientPlayer)
        {
            if (controller) controller.enabled = false;
            if (Animator) Animator.enabled = false;
            if (input) input.enabled = false;
        }
    }

    public override void Start()
    {
        if (!IsClientPlayer)
        {
            DarkRiftAPI.onDataDetailed += RecieveData;
            return;
        }

        if (DarkRiftAPI.isConnected)
        {
            NetworkID = DarkRiftAPI.id;
            PlayerManager.OnPlayerLoadOK += MakePlayer;
            PlayerManager.LoadPlayer();
        }

        base.Start();
        maxJumpVelocity = Mathf.Abs(Gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight);

        cameraController = cameraRigTransform.GetComponent<CameraController>();
    }

    public void Update()
    {
        if (!IsClientPlayer)
            return;

        LocalUpdate();
        NetworkUpdate();
    }

    private void OnApplicationQuit()
    {
        if (!IsClientPlayer)
        {
            DarkRiftAPI.onDataDetailed -= RecieveData;
        }
    }

    #endregion

    #region _Local Functions_

    private void LocalUpdate()
    {
        canMove = Animator.GetBool("canMove");

        HandleMovement();

        if (canMove)
            RotateTransform();

        UpdateAnimationValues();
    }

    private void HandleMovement()
    {
        float m = Mathf.Abs(input.Current.MoveInput.x) + Mathf.Abs(input.Current.MoveInput.z);
        moveAmount = Mathf.Clamp01(m);
        Vector3 moveDirectionNoYChange = Vector3.zero;

        if (canMove)
        {
            moveDirectionNoYChange += moveDirection;
            moveDirectionNoYChange *= moveAmount;
            moveDirectionNoYChange.y = moveDirection.y;
        }

        if (modelObject.transform.localPosition != Vector3.zero)
        {
            moveDirectionNoYChange += modelObject.transform.localPosition;
            modelObject.transform.localPosition = Vector3.zero;
        }

        transform.position += moveDirectionNoYChange * controller.DeltaTime;
    }

    private void RotateTransform()
    {
        Vector3 targetDirection = moveDirection;
        targetDirection.y = 0f;

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion tr = Quaternion.LookRotation(targetDirection);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, controller.DeltaTime * rotateSpeed);
        transform.rotation = targetRotation;
    }

    private void UpdateAnimationValues()
    {
        if (canMove)
        {
            Animator.SetFloat("vertical", moveAmount);
        }
    }

    #endregion

    #region _Network Functions_

    private void NetworkUpdate()
    {
        if (DarkRiftAPI.isConnected)
        {
            // Update the other clients about are new position/rotation

            if (transform.position != _networkPosition)
                DarkRiftAPI.SendMessageToOthers(NT.MoveT, NT.MoveS.Position, transform.position);

            if (transform.rotation != _networkRotation)
                DarkRiftAPI.SendMessageToOthers(NT.MoveT, NT.MoveS.Rotation, transform.rotation);

            _networkPosition = transform.position;
            _networkRotation = transform.rotation;
        }
    }

    private void RecieveData(ushort sender, byte tag, ushort subject, object data)
    {
        // If Client A 'sender' passes info to Client B, only update on B if you are the same from A
        if (sender == _networkID)
        {
            if (tag == NT.MoveT)
            {
                if (subject == NT.MoveS.Position)
                {
                    transform.position = (Vector3)data;
                }

                if (subject == NT.MoveS.Rotation)
                {
                    transform.rotation = (Quaternion)data;
                }
            }
        }
    }

    //
    // Summary:
    //      Makes the client player
    //
    private void MakePlayer(int id, string playerName, string data)
    {
        CreateThisPlayer(id, playerName, data);
        OnCharacterReady?.Invoke();
    }

    //
    // Summary:
    //      Creates the player for this reference of Player
    //
    public void CreateThisPlayer(int sender, int userID, string playerName, string data)
    {
        NetworkID = sender;
        CreateThisPlayer(userID, playerName, data);
    }

    //
    // Summary:
    //      Creates the player for this reference of Player
    //
    public void CreateThisPlayer(int userID, string playerName, string data)
    {
        UserID = userID;
        PlayerName = playerName;
        PlayerData = data;

        int count = 6;
        int k = 0;

        List<string> partStrings = data.ToLookup(c => Mathf.Floor(k++ / count)).Select(e => new String(e.ToArray())).ToList();

        foreach (string partName in partStrings)
            foreach (PartInfo part in Database.Instance.GetEntries<PartInfo>().Where(p => p.Name == partName))
                GameManager.Instance.ResourceManager.LoadAssetAsync<GameObject>(part.Prefab.Entry, (prefab) =>
                {
                    Transform child = transform.Find("Model");
                    if (child == null)
                    {
                        GameObject newObj = new GameObject("Model");
                        newObj.transform.parent = transform;
                        child = newObj.transform;
                    }

                    Instantiate(prefab, child);
                });
    }

    #endregion

    #region _State Management_

    public bool MaintainingGround()
    {
        return controller.CurrentGround.IsGrounded(true, 1f);
    }

    public bool AcquiringGround()
    {
        return controller.CurrentGround.IsGrounded(false, 0.5f);
    }

    public Vector3 LocalMovement()
    {
        Vector3 lookDirection = cameraRigTransform.forward;
        lookDirection.y = 0f;

        Vector3 right = Vector3.Cross(controller.Up, lookDirection);

        Vector3 local = Vector3.zero;

        if (input.Current.MoveInput.x != 0)
        {
            local += right * input.Current.MoveInput.x;
        }

        if (input.Current.MoveInput.z != 0)
        {
            local += lookDirection * input.Current.MoveInput.z;
        }

        return local.normalized;
    }

    public bool HandleJumpState()
    {
        if (input.Current.JumpInput)
        {
            state.CurrentState = new PlayerJumpState(state, this, controller);
            return true;
        }

        return false;
    }

    public bool HandleFallState()
    {
        if (!MaintainingGround())
        {
            state.CurrentState = new PlayerFallState(state, this, controller);
            return true;
        }

        return false;
    }

    public bool HandleMoveState()
    {
        if (input.Current.MoveInput != Vector3.zero)
        {
            if (input.Current.RunInput)
            {
                state.CurrentState = new PlayerRunState(state, this, controller);
                return true;
            }

            state.CurrentState = new PlayerMoveState(state, this, controller);
            return true;
        }

        return false;
    }

    public bool HandleRunState()
    {
        if (input.Current.RunInput)
        {
            state.CurrentState = new PlayerRunState(state, this, controller);
            return true;
        }

        return false;
    }

    public bool HandleIdleState()
    {
        if (input.Current.MoveInput == Vector3.zero)
        {
            state.CurrentState = new PlayerIdleState(state, this, controller);
            return true;
        }
        return false;
    }

    #endregion
}