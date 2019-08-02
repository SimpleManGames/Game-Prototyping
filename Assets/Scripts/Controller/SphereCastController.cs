using System.Linq;
using UnityEngine;

public class SphereCastController : MonoBehaviour
{
    [Header("Collision Info")]
    public LayerMask walkable;

    [SerializeField]
    protected Collider[] ownCollider;

    [SerializeField]
    protected float radius = 0.5f;

    [SerializeField]
    protected CollisionSphere[] spheres =
        new CollisionSphere[3] {
            new CollisionSphere(0, true, false),
            new CollisionSphere(0.5f, false, false),
            new CollisionSphere(1, false, true),
        };

    protected CollisionSphere Feet
    {
        get { return spheres.Where(s => s.isFeet == true).FirstOrDefault(); }
    }
    protected CollisionSphere Head
    {
        get { return spheres.Where(s => s.isHead == true).FirstOrDefault(); }
    }

    protected Vector3 OffsetPosition(float offset)
    {
        Vector3 p;
        p = transform.position;
        p.y += offset;

        return p;
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (CollisionSphere sphere in spheres)
            Gizmos.DrawWireSphere(transform.position + (sphere.offset * Vector3.up), radius);
    }
}