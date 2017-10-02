using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller))]
public class Enemy : Agent
{
    Controller controller;

    // DEBUG
    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

    public override void Start()
    {
        controller = GetComponent<Controller>();
        base.Start();
    }

    private void Update()
    {
        if (CombatManager.Instance.CurrentAgentTurn == this)
        {
            Debug.Log("Currently Enemies turn");

            if (!watch.IsRunning)
                watch.Start();
            
            if (watch.ElapsedMilliseconds >= 1000f)
            {
                watch.Stop();
                watch.Reset();
                CombatManager.Instance.NextTurn();
            }
        }
    }
}