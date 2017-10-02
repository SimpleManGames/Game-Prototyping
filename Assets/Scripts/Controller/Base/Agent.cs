using UnityEngine;

public class Agent : MonoBehaviour, IGravity
{
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
    
    public virtual void Awake()
    {
        state = new StateMachine();
    }

    public virtual void Start()
    {
        _gravity = -(maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        //_gravity = -9.81f;
    }

    public float CalculateJumpSpeed(float jumpHeight, float gravity)
    {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }
}