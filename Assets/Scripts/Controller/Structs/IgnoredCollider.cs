using UnityEngine;
using System.Collections;

public struct IgnoredCollider
{
    public Collider collider;
    public int layer;

    public IgnoredCollider(Collider collider, int layer)
    {
        this.collider = collider;
        this.layer = layer;
    }
}