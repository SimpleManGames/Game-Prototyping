public class PlayerRollState : IState
{
    private Player player;
    private Controller controller;

    public StateMachine State { get; private set; }

    public PlayerRollState(StateMachine state, Player player, Controller controller)
    {
        State = state;

        this.player = player;
        this.controller = controller;
    }

    public void Start()
    {
        //player.Animator.SetBool("canMove", true);
        player.Animator.SetFloat("vertical", player.MoveAmount);
        player.Animator.SetBool("rolling", true);
    }

    public void Update()
    {
        if(!player.Animator.GetBool("rolling"))
            State.Update();
    }

    public void Exit()
    {
        player.Animator.SetBool("rolling", false);
    }

    public bool StateConditional()
    {
        return (player.input.Current.RollInput);
    }
}
