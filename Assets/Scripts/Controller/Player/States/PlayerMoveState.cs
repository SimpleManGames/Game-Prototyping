using UnityEngine;
using System.Collections;
using System;

public class PlayerMoveState : IState
{
    public StateMachine AttachedStateMachine
    {
        get; private set;
    }

    Player player;
    Controller controller;

    float currentSpeed;
    float speedSmoothVelocity;

    public PlayerMoveState(StateMachine attachedStateMachine, Player player, Controller controller)
    {
        AttachedStateMachine = attachedStateMachine;
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
            if (player.input.Current.RunInput)
            {
                player.state.CurrentState = new PlayerRunState(AttachedStateMachine, player, controller);
                return;
            }

            Vector3 dirInputNor = player.input.Current.MoveInput.normalized;
            player.moveDirection = Vector3.MoveTowards(player.moveDirection, player.LocalMovement() * player.moveSpeed, 30.0f * controller.DeltaTime);
        }
        else
        {
            player.state.CurrentState = new PlayerIdleState(AttachedStateMachine, player, controller);
            return;
        }
    }

    public void Exit()
    {
        player.moveDirection.y = 0f;
    }

    private Vector3 LocalMovement()
    {
        Vector3 lookDirection = player.cameraRigTransform.forward;
        lookDirection.y = 0f;

        Vector3 right = Vector3.Cross(controller.Up, lookDirection);

        Vector3 local = Vector3.zero;

        if (player.input.Current.MoveInput.x != 0)
        {
            local += right * player.input.Current.MoveInput.x;
        }

        if (player.input.Current.MoveInput.z != 0)
        {
            local += lookDirection * player.input.Current.MoveInput.z;
        }

        return local.normalized;
    }
}