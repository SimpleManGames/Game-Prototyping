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

        float v = player.input.Current.MoveInput.z;
        float h = player.input.Current.MoveInput.x;

        if (player.lockOn == false)
        {
            v = 1;
            h = 0;
        }
        else
        {
            if (Mathf.Abs(v) < 0.3f)
                v = 0f;
            if (Mathf.Abs(h) < 0.3f)
                h = 0f;
        }

        player.Animator.SetFloat("vertical", v);
        player.Animator.SetFloat("horizontal", h);

        player.rolling = true;
    }

    public void Update()
    {
        if (!player.lockOn)
        {
            player.Animator.SetFloat("vertical", 1);
            player.Animator.SetFloat("horizontal", 0);
        }

        int i = player.Animator.GetLayerIndex("Override");
        if (player.Animator.GetCurrentAnimatorStateInfo(i).IsName("Empty Override"))
            State.CurrentState = new PlayerIdleState(State, player, controller);        
    }

    public void Exit()
    {
        player.Animator.applyRootMotion = false;
        player.rolling = false;
    }
}