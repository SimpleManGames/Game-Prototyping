using System;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase, RequireComponent(typeof(Controller)), RequireComponent(typeof(PlayerInputController))]
public class Player : Agent
{
    [HideInInspector]
    public PlayerInputController input;

    [HideInInspector]
    public Transform cameraRigTransform;

    Controller controller;
    public Animator Animator { get; private set; }
    private GameObject _modelObject;
    public GameObject ModelObject { get { return _modelObject; } }

    #region Movement Info

    [Header("Movement Info")]
    [ReadOnly, Tooltip("Describes the direction the player controller is trying to move. " +
        "If you want to manually move this, use the Controller's debug move")]
    public Vector3 moveDirection;
    [SerializeField, ReadOnly]
    private float _moveAmount;
    public float MoveAmount { get { return _moveAmount; } set { _moveAmount = value; } }
    [ReadOnly] public bool canMove;

    #endregion

    public override void Awake()
    {
        base.Awake();
        controller = GetComponent<Controller>();
        Animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInputController>();
        state.CurrentState = new PlayerIdleState(state, this, controller);

        _modelObject = transform.Find("boxMan").gameObject;
    }

    public override void Start()
    {
        base.Start();
        maxJumpVelocity = Mathf.Abs(Gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight);

        state.AddState(new PlayerIdleState(state, this, controller));
        state.AddState(new PlayerMoveState(state, this, controller));
        state.AddState(new PlayerRunState(state, this, controller));
        state.AddState(new PlayerFallState(state, this, controller));
        state.AddState(new PlayerJumpState(state, this, controller));
        state.AddState(new PlayerRollState(state, this, controller));
    }

    public void Update()
    {
        canMove = Animator.GetBool("canMove");

        if (canMove)
            RotateTransform();

        UpdateAnimationValues();
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
            Animator.SetFloat("vertical", _moveAmount);
    }

    #region State Management

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

    #endregion
}