using UnityEngine;

[RequireComponent(typeof(Controller))]
[RequireComponent(typeof(PlayerInputController))]
public class Player : Agent
{
    [HideInInspector]
    public PlayerInputController input;

    [HideInInspector]
    public Transform cameraRigTransform;

    Controller controller;
    public Animator Animator { get; private set; }

    public Vector3 moveDirection;
    private float moveAmount;

    public override void Awake()
    {
        base.Awake();
        controller = GetComponent<Controller>();
        Animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInputController>();
        state.CurrentState = new PlayerIdleState(state, this, controller);
    }

    public override void Start()
    {
        base.Start();
        maxJumpVelocity = Mathf.Abs(Gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight);
    }

    public void Update()
    {
        transform.position += moveDirection * controller.DeltaTime;
        
        float v = input.Current.MoveInput.z;
        float h = input.Current.MoveInput.x;

        float m = Mathf.Abs(moveDirection.x) + Mathf.Abs(moveDirection.z);
        moveAmount = Mathf.Clamp01(m);

        Vector3 targetDirection = moveDirection;
        targetDirection.y = 0f;

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion tr = Quaternion.LookRotation(targetDirection);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, controller.DeltaTime * rotateSpeed);
        transform.rotation = targetRotation;

        UpdateAnimationValues();
    }

    private void UpdateAnimationValues()
    {
        Animator.SetFloat("vertical", Mathf.Abs(moveAmount));
        Animator.SetFloat("horizontal", Mathf.Abs(input.Current.MoveInput.x));
    }

    public bool MaintainingGround()
    {
        return controller.CurrentGround.IsGrounded(true, 0.5f);
    }

    public bool AcquiringGround()
    {
        return controller.CurrentGround.IsGrounded(false, 0.01f);
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

    private float NormalizeFloat(float value, float min = 0, float max = 1) {
        return (value - min) / (max - min);
    }
}