using UnityEngine;

public class PlayerMoveState : IState
{
    public StateMachine State
    {
        get; private set;
    }

    Player player;
    Controller controller;

    float currentSpeed;
    float speedSmoothVelocity;

    public PlayerMoveState(StateMachine state, Player player, Controller controller)
    {
        State = state;
        this.player = player;
        this.controller = controller;
    }

    public void Start()
    {
        controller.EnableSlopeLimit();
        controller.EnableClamping();
    }

    public void Update()
    {
        if (player.HandleJumpState())
            return;

        if (player.HandleFallState())
            return;
        
        if (!player.HandleIdleState())
        {
            Vector3 dirInputNor = player.input.Current.MoveInput.normalized;
            player.moveDirection = Vector3.MoveTowards(player.moveDirection, player.LocalMovement() * player.moveSpeed, 30.0f * controller.DeltaTime);
        }
        else
            return;
                       
        if (player.HandleRunState())
            return;
    }

    public void Exit()
    {

    }
}