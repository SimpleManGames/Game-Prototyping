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
        player.Animator.CrossFade("Rolls", 0f);

        player.rolling = true;
    }

    public void Update()
    {
        int i = player.Animator.GetLayerIndex("Override");
        bool isRolling = player.Animator.GetCurrentAnimatorStateInfo(i).IsName("Rolling");

        
        if (!player.rolling)
            State.CurrentState = State.PreviousState;
    }

    public void Exit()
    {
        player.Animator.applyRootMotion = false;
        player.rolling = false;
    }
}