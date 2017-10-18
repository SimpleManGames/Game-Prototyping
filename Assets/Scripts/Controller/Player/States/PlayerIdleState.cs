using UnityEngine;

public sealed class PlayerIdleState : IState
{
    private Player player;
    private Controller controller;

    public StateMachine State { get; private set; }

    public PlayerIdleState(StateMachine state, Player player, Controller controller)
    {
        State = state;

        this.player = player;
        this.controller = controller;
    }

    public void Start()
    {
        controller.EnableSlopeLimit();
        controller.EnableClamping();

        player?.Animator?.SetFloat("vertical", 0f);
        player.moveDirection.y = 0f;
    }

    public void Update()
    {
        if (player == null)
            return;

        if (player.HandleJumpState())
            return;

        if (player.HandleFallState())
            return;

        if (player.HandleMoveState())
            return;

        player.moveDirection = Vector3.MoveTowards(player.moveDirection, Vector3.zero, 25f * controller.DeltaTime);
    }

    public void Exit() { }
}