using UnityEngine;

public class PlayerLockOnState : IState
{
    public StateMachine State
    {
        get; private set;
    }

    Controller controller;
    Player player;

    ITargetable targetObject;

    public PlayerLockOnState(StateMachine state, Player player, Controller controller, ITargetable targetObject)
    {
        this.State = state;
        this.controller = controller;
        this.player = player;
        this.targetObject = targetObject;
    }

    public void Start()
    {
        player.Animator.SetBool("lockon", true);
        player.cameraRigTransform.GetComponent<CameraController>().LockOn = true;

        controller.EnableSlopeLimit();
        controller.EnableClamping();

        player.lockOn = true;
    }

    public void Update()
    {
        if (player.HandleFallState())
            return;

        if (player.HandleJumpState())
            return;

        if (player.HandleRollState())
            return;

        if (!player.input.Current.TargetInput)
        {
            if (player.input.Current.RunInput)
            {
                player.state.CurrentState = new PlayerRunState(State, player, controller);
                ExitLockOn();
            }
            else
            {
                player.state.CurrentState = new PlayerMoveState(State, player, controller);
                ExitLockOn();
            }
            return;
        }

        if (player.input.Current.MoveInput != Vector3.zero)
        {
            Vector3 dirInputNor = player.input.Current.MoveInput.normalized;
            player.moveDirection = Vector3.MoveTowards(player.moveDirection, player.LocalMovement() * player.moveSpeed, 30.0f * controller.DeltaTime);
        }
        else
        {
            player.moveDirection = Vector3.MoveTowards(player.moveDirection, Vector3.zero, 50f * controller.DeltaTime);
        }
    }

    public void ExitLockOn()
    {
        player.input.Current.TargetInput = false;
        player.Animator.SetBool("lockon", false);
        player.cameraRigTransform.GetComponent<CameraController>().LockOn = false;
        player.lockOn = false;
    }

    public void Exit()
    {
    }
}