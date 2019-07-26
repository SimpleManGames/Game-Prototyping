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
        Vector3 dirInputNor = player.input.Current.MoveInput.normalized;
        player.moveDirection = Vector3.MoveTowards(player.moveDirection, player.LocalMovement() * player.moveSpeed, 30.0f * controller.DeltaTime);

        float m = Mathf.Abs(player.input.Current.MoveInput.x) + Mathf.Abs(player.input.Current.MoveInput.z);
        player.MoveAmount = Mathf.Clamp01(m);
        Vector3 moveDirectionNoYChange = Vector3.zero;

        if (player.canMove)
        {
            moveDirectionNoYChange += player.moveDirection;
            moveDirectionNoYChange *= player.MoveAmount;
            moveDirectionNoYChange.y = player.moveDirection.y;
        }

        if (player?.ModelObject?.transform.localPosition != Vector3.zero)
        {
            moveDirectionNoYChange += player.ModelObject.transform.localPosition;
            player.ModelObject.transform.localPosition = Vector3.zero;
        }

        player.moveDirection += moveDirectionNoYChange * controller.DeltaTime;
        player.transform.position += player.moveDirection * controller.DeltaTime;

        State.Update();
    }

    public void Exit()
    {
        //player.MoveAmount = 0f;
    }

    public bool StateConditional()
    {
        return (player.input.Current.MoveInput != Vector3.zero) && (!player.input.Current.RunInput);
    }
}