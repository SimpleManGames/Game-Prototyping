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

        player.Animator.SetFloat("vertical", 0f);
        player.moveDirection.y = 0f;
    }

    public void Update()
    {
        player.moveDirection = Vector3.MoveTowards(player.moveDirection, Vector3.zero, 25f * controller.DeltaTime);

        State.Update();
    }

    public void Exit() { }

    public bool StateConditional()
    {
        return (player.input.Current.MoveInput == Vector3.zero);
    }
}