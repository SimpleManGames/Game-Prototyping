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
    private CameraController cameraController;

    Controller controller;
    public Animator Animator { get; private set; }
    private GameObject modelObject;

    #region Movement Info

    [Header("Movement Info")]
    [ReadOnly, Tooltip("Describes the direction the player controller is trying to move. " +
        "If you want to manually move this, use the Controller's debug move")]
    public Vector3 moveDirection;
    [SerializeField, ReadOnly]
    private float moveAmount;
    public float MoveAmount { get; set; }
    [ReadOnly] public bool canMove;
    [ReadOnly] public bool lockOn;
    [ReadOnly] public bool rolling;
    public float rollModifier = 1;
    [ReadOnly] public Vector2 rollInput;

    #endregion

    [SerializeField] private float targetableRange = 20f;
    [SerializeField] private LayerMask targetableLayer;
    [SerializeField, ReadOnly] private List<GameObject> targetables = new List<GameObject>();
    public List<GameObject> Targetables
    {
        get { return targetables; }
    }
    [Obsolete] private float halfVisionConeSize = 45f;

    [Header("Debug")]
    [SerializeField] private bool debugTargets;

    public override void Awake()
    {
        base.Awake();
        controller = GetComponent<Controller>();
        Animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInputController>();
        state.CurrentState = new PlayerIdleState(state, this, controller);

        modelObject = transform.Find("boxMan").gameObject;
    }

    public override void Start()
    {
        base.Start();
        maxJumpVelocity = Mathf.Abs(Gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight);

        cameraController = cameraRigTransform.GetComponent<CameraController>();
    }

    public void Update()
    {
        canMove = Animator.GetBool("canMove");
        rolling = Animator.GetBool("rolling");

        HandleMovement();

        if (canMove)
            RotateTransform();

        UpdateAnimationValues();

        //DetectTargetables();

        CheckTargetRadius();
    }

    private void OnTriggerStay(Collider other)
    {
        int mask = 1 << targetableLayer;
        if (other.gameObject.layer == controller.walkable)
        {
            Debug.Log("Hit mask");
            return;
        }
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
        float v = input.Current.MoveInput.z;
        float h = input.Current.MoveInput.x;

        if (input.Current.RollInput)
        {
            rollInput.y = input.Current.MoveInput.z;
            rollInput.x = input.Current.MoveInput.x;
        }


        if (lockOn)
        {
            if (rolling)
            {
                Animator.SetFloat("vertical", rollInput.y);
                Animator.SetFloat("horizontal", rollInput.x);
                return;
            }

            Animator.SetFloat("vertical", v, 0.2f, controller.DeltaTime);
            Animator.SetFloat("horizontal", h, 0.1f, controller.DeltaTime);
            return;
        }

        if (rolling)
        {
            if (lockOn == false)
            {
                rollInput.y = 1;
                rollInput.x = 0;

                Animator.SetFloat("vertical", rollInput.y);
                Animator.SetFloat("horizontal", rollInput.x);
                return;
            }
            else
            {
                if (Mathf.Abs(rollInput.y) > 0.3f)
                    rollInput.y = 0f;
                if (Mathf.Abs(rollInput.x) > 0.3f)
                    rollInput.x = 0f;

                Animator.SetFloat("vertical", rollInput.y);
                Animator.SetFloat("horizontal", rollInput.x);
                return;
            }
        }

        if (canMove)
        {
            Animator.SetFloat("vertical", moveAmount);
        }
    }

    [Obsolete("This method is no longer used, just want to keep it around")]
    private void DetectTargetablesInCone()
    {
        var hits = Physics.OverlapSphere(transform.position, 25f, targetableLayer, QueryTriggerInteraction.Collide);

        if (hits.Length == 0)
            return;

        foreach (var hit in hits)
        {
            if (hit.gameObject.GetComponent(typeof(ITargetable)))
            {
                Vector3 myPos = transform.position;
                Vector3 myVector = transform.forward;
                Vector3 theirPos = hit.transform.position;
                Vector3 theirVector = theirPos - myPos;

                float mag = Vector3.SqrMagnitude(myVector) * Vector3.SqrMagnitude(theirVector);

                if (mag == 0f)
                    return;

                float dotProd = Vector3.Dot(myVector, theirPos - myPos);
                bool isNegative = dotProd < 0f;
                dotProd *= dotProd;

                if (isNegative)
                    dotProd *= -1;

                float sqrAngle = Mathf.Rad2Deg * Mathf.Acos(dotProd / mag);
                bool isInFront = sqrAngle < halfVisionConeSize;

                Debug.DrawLine(myPos, theirPos, isInFront ? Color.green : Color.red);
                if (isInFront)
                {
                    int mask = 1 << controller.walkable;
                    if (!Physics.Linecast(myPos, theirPos, mask))
                    {
                        Debug.Log("See target " + hit.name);
                    }
                }
            }
        }
    }

    private void CheckTargetRadius()
    {
        targetables.Clear();
        var hits = Physics.OverlapSphere(transform.position, 25f, targetableLayer, QueryTriggerInteraction.Collide);

        if (hits.Length == 0)
            return;

        foreach (var hit in hits)
        {
            ITargetable target = hit.gameObject.GetComponent(typeof(ITargetable)) as ITargetable;
            if (target == null)
                continue;

            if (!target.IsTargetable())
                continue;

            Vector3 playerPosition = transform.position + (Vector3.up * controller.Height / 2);

            if (!Physics.Linecast(playerPosition, target.TargetPosition(), controller.walkable))
                targetables.Add(hit.gameObject);
        }

        if (debugTargets)
            foreach (var target in targetables)
                Debug.DrawLine(transform.position + (Vector3.up * controller.Height / 2), (target.transform.GetComponent(typeof(ITargetable)) as ITargetable).TargetPosition());
    }

    #region State Management

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

    public bool HandleTargetState()
    {
        if (input.Current.TargetInput && Targetables.Count > 0)
        {
            state.CurrentState = new PlayerLockOnState(state, this, controller, null);
            return true;
        }

        input.Current.TargetInput = false;
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

    public bool HandleRollState()
    {
        if (input.Current.RollInput)
        {
            state.CurrentState = new PlayerRollState(state, this, controller);
            return true;
        }

        return false;
    }

    #endregion
}