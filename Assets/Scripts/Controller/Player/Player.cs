using UnityEngine;

[SelectionBase, RequireComponent(typeof(PlayerInputController))]
public class Player : Agent
{
    [HideInInspector]
    public PlayerInputController input;

    [HideInInspector]
    public Transform cameraRigTransform;

    public Animator Animator { get; private set; }
    public GameObject ModelObject { get; private set; }

    public override void Awake()
    {
        base.Awake();
        Animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInputController>();
        state.CurrentState = new PlayerIdleState(state, this, controller);

        ModelObject = transform.Find("boxMan").gameObject;
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
    }

    public void Update()
    {
        canMove = Animator.GetBool("canMove");

        UpdateAnimationValues();
    }

    private void UpdateAnimationValues()
    {
        if (canMove)
            Animator.SetFloat("vertical", _moveAmount);
    }

    #region State Management

    public bool MaintainingGround() => controller.CurrentGround.IsGrounded(true, 1f);
    public bool AcquiringGround() => controller.CurrentGround.IsGrounded(false, 0.5f);

    public Vector3 LocalMovement()
    {
        Vector3 lookDirection = cameraRigTransform.forward;
        lookDirection.y = 0f;

        Vector3 right = Vector3.Cross(controller.Up, lookDirection);

        Vector3 local = Vector3.zero;

        if (input.Current.MoveInput.x != 0)
            local += right * input.Current.MoveInput.x;

        if (input.Current.MoveInput.z != 0)
            local += lookDirection * input.Current.MoveInput.z;

        return local.normalized;
    }

    #endregion
}