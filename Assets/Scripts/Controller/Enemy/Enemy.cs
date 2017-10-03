using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase, RequireComponent(typeof(Controller))]
public class Enemy : Agent, ITargetable
{
    Controller controller;

    // DEBUG
    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

    [SerializeField]
    private float targetOffset;
    public float TargetOffset
    {
        get { return targetOffset; }
    }

    public override void Start()
    {
        base.Start();
        controller = GetComponent<Controller>();
    }

    public Vector3 TargetPosition()
    {
        return transform.position + (Vector3.up * targetOffset);
    }
}