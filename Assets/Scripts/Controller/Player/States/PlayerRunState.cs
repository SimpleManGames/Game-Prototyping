using UnityEngine;

public class PlayerRunState : IState
{
    public StateMachine State
    {
        get; private set;
    }

    Player player;
    Controller controller;

    public PlayerRunState(StateMachine attachedStateMachine, Player player, Controller controller)
    {
        this.State = attachedStateMachine;
        this.player = player;
        this.controller = controller;
    }

    public void Start()
    {
        player.Animator.SetBool("run", true);

        controller.EnableSlopeLimit();
        controller.EnableClamping();
    }

    public void Update()
    {
        if (player.HandleJumpState())
            return;

        if (player.HandleFallState())
            return;

        if (player.HandleIdleState())
            return;

        Vector3 dirInputNor = player.input.Current.MoveInput.normalized;
        player.moveDirection = Vector3.MoveTowards(player.moveDirection, player.LocalMovement() * player.runSpeed, 30.0f * controller.DeltaTime);

        if (player.HandleMoveState())
            return;
    }

    public void Exit()
    {
        player.Animator.SetBool("run", false);
    }
}