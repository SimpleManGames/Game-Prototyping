using UnityEngine;

public sealed class PlayerIdleState : IState
{
    private Player player;
    private Controller controller;

    public StateMachine AttachedStateMachine { get; private set; }

    public PlayerIdleState(StateMachine attachedStateMachine, Player player, Controller controller)
    {
        AttachedStateMachine = attachedStateMachine;

        this.player = player;
        this.controller = controller;
    }

    public void Start()
    {
        controller.EnableSlopeLimit();
        controller.EnableClamping();

        player.moveDirection.y = 0f;
    }

    public void Update()
    {
        if (player.input.Current.JumpInput)
        {
            AttachedStateMachine.CurrentState = new PlayerJumpState(AttachedStateMachine, player, controller);
            return;
        }

        if (!player.MaintainingGround())
        {
            AttachedStateMachine.CurrentState = new PlayerFallState(AttachedStateMachine, player, controller);
            return;
        }

        if (player.input.Current.MoveInput != Vector3.zero)
        {
            player.state.CurrentState = new PlayerMoveState(player.state, player, controller);
        }

        player.moveDirection = Vector3.MoveTowards(player.moveDirection, Vector3.zero, 25f * controller.DeltaTime);
    }

    public void Exit() { }
}