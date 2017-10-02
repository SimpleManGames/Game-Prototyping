using System;
using UnityEngine;

public class PlayerRunState : IState
{
    public StateMachine AttachedStateMachine
    {
        get; private set;
    }

    Player player;
    Controller controller;

    public PlayerRunState(StateMachine attachedStateMachine, Player player, Controller controller)
    {
        this.AttachedStateMachine = attachedStateMachine;
        this.player = player;
        this.controller = controller;
    }


    public void Start()
    {
        player.Animator.SetBool("run", true);
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
            if (!player.input.Current.RunInput)
            {
                player.state.CurrentState = new PlayerMoveState(AttachedStateMachine, player, controller);
                return;
            }

            Vector3 dirInputNor = player.input.Current.MoveInput.normalized;
            player.moveDirection = Vector3.MoveTowards(player.moveDirection, player.LocalMovement() * player.runSpeed, 30.0f * controller.DeltaTime);
        }
        else
        {
            player.state.CurrentState = new PlayerIdleState(AttachedStateMachine, player, controller);
            return;
        }
    }

    public void Exit()
    {
        player.Animator.SetBool("run", false);
    }
}