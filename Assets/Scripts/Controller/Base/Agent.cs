using UnityEngine;

[RequireComponent(typeof(Controller))]
public class Agent : MonoBehaviour, IGravity
{
    protected Controller controller;

    [SerializeField, ReadOnly]
    protected float _gravity;
    public float Gravity { get { return _gravity; } }
    public bool IsStatic { get; set; } = false;

    [Header("Stats")]
    public float moveSpeed = 6f;
    public float runSpeed = 9f;

    public float minJumpHeight = 1f;
    public float maxJumpHeight = 4f;
    public float timeToJumpApex = 0.4f;

    public float rotateSpeed = 5f;

    protected float accelerationTimeAirborne = 0.2f;
    protected float accelerationTimeGrounded = 0.1f;

    protected float minJumpVelocity;
    protected float maxJumpVelocity;

    [Range(0, 1)]
    public float airControlPercent;

    public StateMachine state;

    #region Movement Info

    [Header("Movement Info")]
    [ReadOnly, Tooltip("Describes the direction the player controller is trying to move. " +
        "If you want to manually move this, use the Controller's debug move")]
    public Vector3 moveDirection;
    [SerializeField, ReadOnly]
    protected float _moveAmount;
    public float MoveAmount { get { return _moveAmount; } set { _moveAmount = value; } }
    [ReadOnly] public bool canMove;

    #endregion

    public virtual void Awake()
    {
        controller = GetComponent<Controller>();
        state = state ?? new StateMachine();
    }

    public virtual void Start()
    {
        _gravity = -(maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
    }

    public float CalculateJumpSpeed(float jumpHeight, float gravity)
    {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    private void AdjustLookDirection()
    {
        Vector3 targetDirection = moveDirection;
        targetDirection.y = 0f;

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion tr = Quaternion.LookRotation(targetDirection);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, controller.DeltaTime * rotateSpeed);
        transform.rotation = targetRotation;
    }

    public virtual void EarlyAgentUpdate()
    {
        // Put any code in here you want to run BEFORE the state's update function.
        // This is run regardless of what state you're in

    }

    public virtual void LateAgentUpdate()
    {
        // Put any code in here you want to run AFTER the state's update function.
        // This is run regardless of what state you're in

        transform.position += moveDirection * controller.DeltaTime;
        AdjustLookDirection();
    }
}