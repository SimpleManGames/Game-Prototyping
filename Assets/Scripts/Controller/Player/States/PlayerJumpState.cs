using UnityEngine;

public class PlayerJumpState : IState
{
    public StateMachine AttachedStateMachine
    {
        get; private set;
    }

    Player player;
    Controller controller;

    float currentSpeed;
    float speedSmoothVelocity;

    public PlayerJumpState(StateMachine asm, Player p, Controller c)
    {
        AttachedStateMachine = asm;
        player = p;
        controller = c;
    }

    public void Start()
    {
        controller.DisableClamping();
        controller.DisableSlopeLimit();

        player.moveDirection += controller.Up * player.CalculateJumpSpeed(player.maxJumpHeight, -player.Gravity);

        player.Animator.SetBool("jump", true);
        player.Animator.CrossFade("jump_launch", 0.0f);
    }

    public void Update()
    {

        Vector3 planarMoveDirection = Math3D.ProjectVectorOnPlane(controller.Up, player.moveDirection);
        Vector3 verticalMoveDirection = player.moveDirection - planarMoveDirection;

        if (Vector3.Angle(verticalMoveDirection, controller.Up) > 90 && player.AcquiringGround())
        {
            player.moveDirection = planarMoveDirection;
            AttachedStateMachine.CurrentState = new PlayerIdleState(AttachedStateMachine, player, controller);
            return;
        }

        planarMoveDirection = Vector3.MoveTowards(planarMoveDirection, player.LocalMovement() * player.moveSpeed, 10f * controller.DeltaTime);
        verticalMoveDirection += controller.Up * player.Gravity * controller.DeltaTime;

        player.moveDirection = planarMoveDirection + verticalMoveDirection;
    }

    public void Exit()
    {
        player.Animator.SetBool("jump", false);
        player.Animator.CrossFade("jump_land", .0f);
    }
}