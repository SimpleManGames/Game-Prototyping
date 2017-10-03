public class PlayerFallState : IState
{
    public StateMachine State { get; private set; }

    Player player;
    Controller controller;

    public PlayerFallState(StateMachine attachedStateMachine, Player player, Controller controller)
    {
        State = attachedStateMachine;
        this.player = player;
        this.controller = controller;
    }

    public void Start()
    {
        controller.DisableClamping();
        controller.DisableSlopeLimit();

        player.Animator.SetBool("onGround", false);
        player.Animator.SetFloat("fallFlip", -player.Animator.GetFloat("fallFlip"));
    }

    public void Update()
    {
        if (player.AcquiringGround())
        {
            player.moveDirection = Math3D.ProjectVectorOnPlane(controller.Up, player.moveDirection);
            State.CurrentState = new PlayerIdleState(State, player, controller);
            return;
        }

        player.moveDirection += controller.Up * player.Gravity * controller.DeltaTime;
    }

    public void Exit()
    {
        player.Animator.SetBool("onGround", true);

        controller.EnableSlopeLimit();
        controller.EnableClamping();
    }
}