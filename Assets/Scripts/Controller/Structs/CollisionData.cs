using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CollisionData
{
    public CollisionSphere collisionSphere;
    public CollisionType collisionType;
    public GameObject gameObject;
    public Vector3 point;
    public Vector3 normal;
}