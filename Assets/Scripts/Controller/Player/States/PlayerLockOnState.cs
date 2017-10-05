using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerLockOnState : IState
{
    public StateMachine State
    {
        get; private set;
    }

    Controller controller;
    Player player;

    CameraController cameraController;
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
        cameraController = player.cameraRigTransform.GetComponent<CameraController>();

        if (cameraController.LockOnTarget == null)
            cameraController.LockOnTarget = player.Targetables.First().transform;
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

        // TODO: Implement an option for keyboard mouse
        if (player.input.Current.RightStickInput != Vector2.zero && player.input.previous.RightStickNeutral)
        {
            List<GameObject> potentialTargetChanges = new List<GameObject>();
            foreach (var target in player.Targetables)
            {
                if (target.transform == cameraController.LockOnTarget)
                {
                    continue;
                }

                Vector3 relativePoint = player.transform.InverseTransformPoint(target.transform.position);
                if (Mathf.Sign(relativePoint.x) == Mathf.Sign(player.input.Current.RightStickInput.x))
                {
                    potentialTargetChanges.Add(target);
                }
            }

            if (potentialTargetChanges.Count == 0)
                return;

            float closestDistance = float.MaxValue;
            Transform closest = null;

            foreach (var potentialTarget in potentialTargetChanges)
            {
                float distCheck = Mathf.Abs(Vector3.Distance(cameraController.LockOnTarget.position, potentialTarget.transform.position));
                if (distCheck < closestDistance)
                {
                    closest = potentialTarget.transform;
                    closestDistance = distCheck;
                }
            }

            Debug.DrawLine(cameraController.LockOnTarget.position, closest.position, Color.yellow, 2f);
            
            if (closest != null)
                cameraController.LockOnTarget = closest;
        }
    }

    public void ExitLockOn()
    {
        player.input.Current.TargetInput = false;
        player.Animator.SetBool("lockon", false);
        cameraController.LockOnTarget = null;
        cameraController.LockOn = false;
        player.lockOn = false;
    }

    public void Exit()
    {
    }
}