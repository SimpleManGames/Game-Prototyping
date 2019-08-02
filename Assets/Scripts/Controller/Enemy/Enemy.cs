using Core.Managers;
using UnityEngine;

[SelectionBase, RequireComponent(typeof(Controller))]
public class Enemy : Agent, ITargetable
{
    [SerializeField]
    private float targetOffset;
    public float TargetOffset
    {
        get { return targetOffset; }
    }

    public bool IsTargetable()
    {
        Vector3 screenPoint = GameManager.Instance.Camera.WorldToViewportPoint(TargetPosition());
        return screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }

    public override void Start()
    {
        base.Start();
        controller = GetComponent<Controller>();
    }

    private void Update()
    {
        
    }

    public Vector3 TargetPosition()
    {
        return transform.position + (Vector3.up * targetOffset);
    }
}