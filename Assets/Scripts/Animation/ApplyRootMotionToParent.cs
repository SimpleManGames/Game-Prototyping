using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyRootMotionToParent : MonoBehaviour
{

    public Transform parentObject;
    private Player player;
    private Animator animator;

    private void Start()
    {
        if (parentObject == null)
            parentObject = transform.parent;

        player = parentObject.GetComponent<Player>();

        animator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
        if (player?.canMove == false)
        {
            parentObject.position += animator.deltaPosition;
        }
    }
}