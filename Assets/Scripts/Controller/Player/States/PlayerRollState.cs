using System;
using UnityEngine;

class PlayerRollState : IState
{
    public StateMachine State { get; }

    Player player;
    Controller controller;
    
    public PlayerRollState(StateMachine state, Player player, Controller controller)
    {
        State = state;
        this.player = player;
        this.controller = controller;
    }

    public void Start()
    {
        player.Animator.applyRootMotion = true;
        player.Animator.CrossFade("Rolls", 0.15f);


    }

    public void Update()
    {
        int i = player.Animator.GetLayerIndex("Override");
        if (!player.rolling)
            State.CurrentState = new PlayerIdleState(State, player, controller);
    }

    public void Exit()
    {
        player.Animator.applyRootMotion = false;
        player.Animator.SetBool("rolling", false);
    }
}