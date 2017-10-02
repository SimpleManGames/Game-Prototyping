using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]
public class Controller : SphereCastController
{
    [SerializeField]
    private QueryTriggerInteraction triggerInteraction;

    public delegate void UpdateDelegate();
    public event UpdateDelegate AfterSingleUpdate;

    [SerializeField]
    private bool clampToMovingGround;

    [Header("Update Style")]
    #region Update Style

    [SerializeField]
    private int fixedUpdatePerSecond;
    [SerializeField]
    private bool fixedTimeStep;

    #endregion

    [Header("Debug Variables")]
    #region Debug Variables


    [SerializeField]
    private Vector3 debugMove;

    [SerializeField]
    private bool debugSpheres;

    [SerializeField]
    private bool debugGrounding;

    [SerializeField]
    private bool debugCollisionSteps;

    [SerializeField]
    private bool debugPushbackMessages;

    #endregion

    #region Private Varibles

    #region ReadOnly

    [SerializeField, ReadOnly]
    protected bool clamping = true;
    [SerializeField, ReadOnly]
    private bool slopeLimiting = true;

    #endregion

    private Agent agent;

    private Vector3 initialPosition;
    private Vector3 groundOffset;
    private Vector3 lastGroundPosition;

    private float hRadius;
    
    private int TemporaryLayerIndex;
    private float fixedDeltaTime;

    private static CollisionType defaultCollisionType;
    private List<Collider> ignoredColliders;
    private List<IgnoredCollider> ignoredColliderStack;

    private const int MaxPushbackIterations = 2;
    private const float IsGroundedTolerance = 0.05f;
    private const float Tolerance = 0.05f;
    private const float TinyTolerance = 0.02f;
    private const string TemporaryLayer = "TempCast";

    #endregion

    #region Properties

    // Collision info
    public List<CollisionData> CollisionData { get; private set; }
    public Ground CurrentGround { get; private set; }
    public Transform CurrentlyClampTo { get; set; }

    // Height
    public float Height { get { return Vector3.Distance(SpherePosition(Head), SpherePosition(Feet)) + radius * 2; } }
    public float HeightScale { get; set; }

    public Vector3 Up { get { return transform.up; } }
    public Vector3 Down { get { return -Up; } }

    // Update
    public bool ManualUpdateOnly { get; set; }
    public float DeltaTime { get; private set; }

    #endregion
    
    #region Unity API

    private void Awake()
    {
        CollisionData = new List<CollisionData>();

        TemporaryLayerIndex = LayerMask.NameToLayer(TemporaryLayer);

        ignoredColliders = new List<Collider>();
        ignoredColliderStack = new List<IgnoredCollider>();

        CurrentlyClampTo = null;

        fixedDeltaTime = 1.0f / fixedUpdatePerSecond;

        HeightScale = 1.0f;

        hRadius = radius / 2;

        if (ownCollider)
            IgnoreCollider(ownCollider);

        if (defaultCollisionType == null)
            defaultCollisionType = new GameObject("DefaultCollisionType", typeof(CollisionType)).GetComponent<CollisionType>();

        agent = GetComponent<Agent>();

        CurrentGround = new Ground(walkable, this, triggerInteraction);

        ManualUpdateOnly = false;
    }

    protected override void OnDrawGizmos()
    {
        if (debugSpheres)
            base.OnDrawGizmos();
    }

    #endregion

    #region Update Functions

    private void Update()
    {
        if (ManualUpdateOnly)
            return;

        if (!fixedTimeStep)
        {
            DeltaTime = Time.deltaTime;

            SingleUpdate();
        }
        else
        {
            float delta = Time.deltaTime;

            while (delta > fixedDeltaTime)
            {
                DeltaTime = fixedDeltaTime;

                SingleUpdate();

                delta -= fixedDeltaTime;
            }

            if (delta > 0f)
            {
                DeltaTime = delta;
                SingleUpdate();
            }
        }
    }

    public void ManualUpdate(float deltaTime)
    {
        this.DeltaTime = deltaTime;
        SingleUpdate();
    }

    private void SingleUpdate()
    {
        bool isClamping = clamping || CurrentlyClampTo != null;
        Transform clampedTo = CurrentlyClampTo != null ? CurrentlyClampTo : CurrentGround.transform;

        if (clampToMovingGround && isClamping && clampedTo != null && clampedTo.position - lastGroundPosition != Vector3.zero)
        {
            transform.position += clampedTo.position - lastGroundPosition;
        }

        initialPosition = transform.position;

        ProbeGround(1);

        transform.position += debugMove * DeltaTime;

        agent.state.CurrentState.Update();

        CollisionData.Clear();
        
        // Calculate the distance the character is trying to move
        Vector3 vDistance = transform.position - initialPosition;
        float distance = vDistance.magnitude;

        // Calculate the number of steps to consider during the collision detection relatively to the player radius
        // If the player moves less than half its radius during a frame, only one step is needed
        // If the player moves more than that we subdivide the movement of the charactor in various steps and we test each of the until a collision is found

        int steps = (int)(distance / hRadius) + 1;

        for (int i = 0; i < steps; i++)
        {
            float testDistance = i * hRadius + distance % hRadius;

            Vector3 testPosition = initialPosition + (vDistance.normalized * testDistance);

            transform.position = testPosition;
            
            if (RecursivePushback(0, MaxPushbackIterations))
            {
                if (debugCollisionSteps)
                    Debug.Log("Collision detected on step " + (i + 1) + " of " + steps);

                break;
            }
        }

        ProbeGround(2);

        if (slopeLimiting)
            SlopeLimit();

        ProbeGround(3);

        if (clamping)
            ClampToGround();

        isClamping = clamping || CurrentlyClampTo != null;
        clampedTo = CurrentlyClampTo != null ? CurrentlyClampTo : CurrentGround.transform;

        if (isClamping)
            lastGroundPosition = clampedTo.position;

        if (debugGrounding)
            CurrentGround.DebugGround(true, true, true, true, true);

        AfterSingleUpdate?.Invoke();
    }

    #endregion

    private void ProbeGround(int iter)
    {
        PushIgnoredColliders();
        CurrentGround.ProbeGround(SpherePosition(Feet), iter);
        PopIgnoredColliders();
    }

    private bool SlopeLimit()
    {
        Vector3 n = CurrentGround.PrimaryNormal();
        float a = Vector3.Angle(n, Up);

        if (a > CurrentGround.collisionType.slopeLimit)
        {
            Vector3 absoluteMoveDirection = Math3D.ProjectVectorOnPlane(n, transform.position - initialPosition);

            Vector3 r = Vector3.Cross(n, Down);
            Vector3 v = Vector3.Cross(r, n);

            float angle = Vector3.Angle(absoluteMoveDirection, v);

            if (angle <= 90.0f)
                return false;

            Vector3 resolvedPosition = Math3D.ProjectPointOnLine(initialPosition, r, transform.position);
            Vector3 direction = Math3D.ProjectVectorOnPlane(n, resolvedPosition - transform.position);

            RaycastHit hit;

            if (Physics.CapsuleCast(SpherePosition(Feet), SpherePosition(Head), radius, direction.normalized, out hit, direction.magnitude, walkable, triggerInteraction))
            {
                transform.position += v.normalized * hit.distance;
            }
            else
            {
                transform.position += direction;
            }

            return true;
        }

        return false;
    }

    bool RecursivePushback(int depth, int maxDepth)
    {
        bool hasCollided = false;

        PushIgnoredColliders();

        CollisionData.Clear();

        bool contact = false;

        foreach (var sphere in spheres)
        {
            foreach (Collider col in Physics.OverlapSphere((SpherePosition(sphere)), radius, walkable, triggerInteraction))
            {
                Vector3 position = SpherePosition(sphere);
                Vector3 contactPoint;
                bool contactPointSuccess = ColliderMethods.ClosestPointOnSurface(col, position, radius, out contactPoint);

                if (contactPointSuccess)
                {
                    if (debugPushbackMessages)
                        DebugDraw.DrawMarker(contactPoint, 2.0f, Color.cyan, 0.0f, false);

                    Vector3 v = contactPoint - position;
                    if (v != Vector3.zero)
                    {
                        // Cache the collider's layer so that we can cast against it
                        int layer = col.gameObject.layer;

                        col.gameObject.layer = TemporaryLayerIndex;

                        // Check which side of the normal we are on
                        bool facingNormal = Physics.SphereCast(new Ray(position, v.normalized), TinyTolerance, v.magnitude + TinyTolerance, 1 << TemporaryLayerIndex);

                        col.gameObject.layer = layer;

                        // Orient and scale our vector based on which side of the normal we are situated
                        if (facingNormal)
                        {
                            if (Vector3.Distance(position, contactPoint) < radius)
                            {
                                v = v.normalized * (radius - v.magnitude) * -1;
                            }
                            else
                            {
                                // A previously resolved collision has had a side effect that moved us outside this collider
                                continue;
                            }
                        }
                        else
                        {
                            v = v.normalized * (radius + v.magnitude);
                        }

                        contact = true;

                        transform.position += v;

                        col.gameObject.layer = TemporaryLayerIndex;

                        // Retrieve the surface normal of the collided point
                        RaycastHit normalHit;

                        Physics.SphereCast(new Ray(position + v, contactPoint - (position + v)), TinyTolerance, out normalHit, 1 << TemporaryLayerIndex);

                        col.gameObject.layer = layer;

                        CollisionType superColType = col.gameObject.GetComponent<CollisionType>();

                        if (superColType == null)
                            superColType = defaultCollisionType;

                        // Our collision affected the collider; add it to the collision data
                        var collision = new CollisionData()
                        {
                            collisionSphere = sphere,
                            collisionType = superColType,
                            gameObject = col.gameObject,
                            point = contactPoint,
                            normal = normalHit.normal
                        };

                        CollisionData.Add(collision);
                    }
                }

                if (contact)
                    hasCollided = true;
            }
        }

        PopIgnoredColliders();

        if (depth < maxDepth && contact)
        {
            RecursivePushback(depth + 1, maxDepth);
        }

        return hasCollided;
    }

    private void ClampToGround()
        => transform.position -= Up * CurrentGround.Distance();

    #region Enable/Disable Functions

    public void EnableClamping()
        => clamping = true;

    public void DisableClamping()
        => clamping = false;

    public void EnableSlopeLimit()
        => slopeLimiting = true;

    public void DisableSlopeLimit()
        => slopeLimiting = false;

    #endregion

    public Vector3 SpherePosition(CollisionSphere sphere)
    {
        if (sphere.isFeet)
            return transform.position + sphere.offset * Up;
        else
            return transform.position + sphere.offset * Up * HeightScale;
    }
    
    #region Ignore Collider Functions

    public void IgnoreCollider(Collider col)
    {
        ignoredColliders.Add(col);
    }

    public void RemoveIgnoredCollider(Collider col)
    {
        ignoredColliders.Remove(col);
    }

    private void PushIgnoredColliders()
    {
        ignoredColliderStack.Clear();

        for (int i = 0; i < ignoredColliderStack.Count; i++)
        {
            Collider col = ignoredColliders[i];
            ignoredColliderStack.Add(new IgnoredCollider(col, col.gameObject.layer));
            col.gameObject.layer = TemporaryLayerIndex;
        }
    }

    private void PopIgnoredColliders()
    {
        for (int i = 0; i < ignoredColliderStack.Count; i++)
        {
            IgnoredCollider ic = ignoredColliderStack[i];
            ic.collider.gameObject.layer = ic.layer;
        }

        ignoredColliderStack.Clear();
    }

    #endregion

    public class Ground
    {
        private LayerMask walkable;
        private Controller controller;
        private QueryTriggerInteraction triggerInteraction;

        public Ground(LayerMask walkable, Controller controller, QueryTriggerInteraction triggerInteraction)
        {
            this.walkable = walkable;
            this.controller = controller;
            this.triggerInteraction = triggerInteraction;
        }

        private class GroundHit
        {
            public Vector3 Point { get; private set; }
            public Vector3 Normal { get; private set; }
            public float Distance { get; private set; }

            public GroundHit(Vector3 point, Vector3 normal, float distance)
            {
                Point = point;
                Normal = normal;
                Distance = distance;
            }
        }

        private GroundHit primaryGround;
        private GroundHit nearGround;
        private GroundHit farGround;
        private GroundHit stepGround;
        private GroundHit flushGround;

        public CollisionType collisionType { get; private set; }
        public Transform transform { get; private set; }

        private const float groundingUpperBoundAngle = 60.0f;
        private const float groundingMaxPercentFromCenter = 0.85f;
        private const float groundingMinPercentFromCenter = 0.50f;

        public void ProbeGround(Vector3 origin, int iter)
        {
            ResetGrounds();

            Vector3 up = controller.Up;
            Vector3 down = -up;

            Vector3 o = origin + (up * Tolerance);

            // Reduce our radius by Tolerance squared to avoid failing the SphereCast due to clipping with walls
            float smallerRadius = controller.radius - (Tolerance * Tolerance);

            RaycastHit hit;

            if (Physics.SphereCast(o, smallerRadius, down, out hit, Mathf.Infinity, walkable, triggerInteraction))
            {
                var colType = hit.collider.gameObject.GetComponent<CollisionType>();

                if (colType == null)
                {
                    colType = defaultCollisionType;
                }

                collisionType = colType;
                transform = hit.transform;

                // By reducing the initial SphereCast's radius by Tolerance, our casted sphere no longer fits with
                // our controller's shape. Reconstruct the sphere cast with the proper radius
                SimulateSphereCast(hit.normal, out hit);

                primaryGround = new GroundHit(hit.point, hit.normal, hit.distance);

                // If we are standing on a perfectly flat surface, we cannot be either on an edge,
                // On a slope or stepping off a ledge
                if (Vector3.Distance(Math3D.ProjectPointOnPlane(controller.Up, controller.transform.position, hit.point), controller.transform.position) < TinyTolerance)
                {
                    return;
                }

                // As we are standing on an edge, we need to retrieve the normals of the two
                // faces on either side of the edge and store them in nearHit and farHit

                Vector3 toCenter = Math3D.ProjectVectorOnPlane(up, (controller.transform.position - hit.point).normalized * TinyTolerance);

                Vector3 awayFromCenter = Quaternion.AngleAxis(-80.0f, Vector3.Cross(toCenter, up)) * -toCenter;

                Vector3 nearPoint = hit.point + toCenter + (up * TinyTolerance);
                Vector3 farPoint = hit.point + (awayFromCenter * 3);

                RaycastHit nearHit;
                RaycastHit farHit;

                Physics.Raycast(nearPoint, down, out nearHit, Mathf.Infinity, walkable, triggerInteraction);
                Physics.Raycast(farPoint, down, out farHit, Mathf.Infinity, walkable, triggerInteraction);

                nearGround = new GroundHit(nearHit.point, nearHit.normal, nearHit.distance);
                farGround = new GroundHit(farHit.point, farHit.normal, farHit.distance);

                // If we are currently standing on ground that should be counted as a wall,
                // we are likely flush against it on the ground. Retrieve what we are standing on
                if (Vector3.Angle(hit.normal, up) > colType.standAngle)
                {
                    // Retrieve a vector pointing down the slope
                    Vector3 r = Vector3.Cross(hit.normal, down);
                    Vector3 v = Vector3.Cross(r, hit.normal);

                    Vector3 flushOrigin = hit.point + hit.normal * TinyTolerance;

                    RaycastHit flushHit;

                    if (Physics.Raycast(flushOrigin, v, out flushHit, Mathf.Infinity, walkable, triggerInteraction))
                    {
                        RaycastHit sphereCastHit;

                        if (SimulateSphereCast(flushHit.normal, out sphereCastHit))
                        {
                            flushGround = new GroundHit(sphereCastHit.point, sphereCastHit.normal, sphereCastHit.distance);
                        }
                        else
                        {
                            // Uh oh
                        }
                    }
                }

                // If we are currently standing on a ledge then the face nearest the center of the
                // controller should be steep enough to be counted as a wall. Retrieve the ground
                // it is connected to at it's base, if there exists any
                if (Vector3.Angle(nearHit.normal, up) > colType.standAngle || nearHit.distance > Tolerance)
                {
                    CollisionType col = null;

                    if (nearHit.collider != null)
                    {
                        col = nearHit.collider.gameObject.GetComponent<CollisionType>();
                    }

                    if (col == null)
                    {
                        col = defaultCollisionType;
                    }

                    // We contacted the wall of the ledge, rather than the landing. Raycast down
                    // the wall to retrieve the proper landing
                    if (Vector3.Angle(nearHit.normal, up) > col.standAngle)
                    {
                        // Retrieve a vector pointing down the slope
                        Vector3 r = Vector3.Cross(nearHit.normal, down);
                        Vector3 v = Vector3.Cross(r, nearHit.normal);

                        RaycastHit stepHit;

                        if (Physics.Raycast(nearPoint, v, out stepHit, Mathf.Infinity, walkable, triggerInteraction))
                        {
                            stepGround = new GroundHit(stepHit.point, stepHit.normal, stepHit.distance);
                        }
                    }
                    else
                    {
                        stepGround = new GroundHit(nearHit.point, nearHit.normal, nearHit.distance);
                    }
                }
            }
            // If the initial SphereCast fails, likely due to the controller clipping a wall,
            // fallback to a raycast simulated to SphereCast data
            else if (Physics.Raycast(o, down, out hit, Mathf.Infinity, walkable, triggerInteraction))
            {
                var colType = hit.collider.gameObject.GetComponent<CollisionType>();

                if (colType == null)
                {
                    colType = defaultCollisionType;
                }

                collisionType = colType;
                transform = hit.transform;

                RaycastHit sphereCastHit;

                if (SimulateSphereCast(hit.normal, out sphereCastHit))
                {
                    primaryGround = new GroundHit(sphereCastHit.point, sphereCastHit.normal, sphereCastHit.distance);
                }
                else
                {
                    primaryGround = new GroundHit(hit.point, hit.normal, hit.distance);
                }
            }
            else
            {
                Debug.LogError("[Controller]: No ground was found below the agent; agent has escaped level");
            }
        }

        private bool SimulateSphereCast(Vector3 groundNormal, out RaycastHit hit)
        {
            float groundAngle = Vector3.Angle(groundNormal, controller.Up) * Mathf.Deg2Rad;

            Vector3 secondaryOrigin = controller.transform.position + controller.Up * Tolerance;

            if (!Mathf.Approximately(groundAngle, 0))
            {
                float horizontal = Mathf.Sin(groundAngle) * controller.radius;
                float vertical = (1.0f - Mathf.Cos(groundAngle)) * controller.radius;

                // Retrieve a vector pointing up the slope
                Vector3 r2 = Vector3.Cross(groundNormal, controller.Down);
                Vector3 v2 = -Vector3.Cross(r2, groundNormal);

                secondaryOrigin += Math3D.ProjectVectorOnPlane(controller.Up, v2).normalized * horizontal + controller.Up * vertical;
            }

            if (Physics.Raycast(secondaryOrigin, controller.Down, out hit, Mathf.Infinity, walkable, triggerInteraction))
            {
                // Remove the tolerance from the distance travelled
                hit.distance -= Tolerance + TinyTolerance;

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsGrounded(bool currentlyGrounded, float distance)
        {
            Vector3 n;
            return IsGrounded(currentlyGrounded, distance, out n);
        }

        public bool IsGrounded(bool currentlyGrounded, float distance, out Vector3 groundNormal)
        {
            groundNormal = Vector3.zero;

            if (primaryGround == null || primaryGround.Distance > distance)
            {
                return false;
            }

            // Check if we are flush against a wall
            if (farGround != null && Vector3.Angle(farGround.Normal, controller.Up) > collisionType.standAngle)
            {
                if (flushGround != null && Vector3.Angle(flushGround.Normal, controller.Up) < collisionType.standAngle && flushGround.Distance < distance)
                {
                    groundNormal = flushGround.Normal;
                    return true;
                }

                return false;
            }

            // Check if we are at the edge of a ledge, or on a high angle slope
            if (farGround != null && !OnSteadyGround(farGround.Normal, primaryGround.Point))
            {
                // Check if we are walking onto steadier ground
                if (nearGround != null && nearGround.Distance < distance && Vector3.Angle(nearGround.Normal, controller.Up) < collisionType.standAngle && !OnSteadyGround(nearGround.Normal, nearGround.Point))
                {
                    groundNormal = nearGround.Normal;
                    return true;
                }

                // Check if we are on a step or stair
                if (stepGround != null && stepGround.Distance < distance && Vector3.Angle(stepGround.Normal, controller.Up) < collisionType.standAngle)
                {
                    groundNormal = stepGround.Normal;
                    return true;
                }

                return false;
            }


            if (farGround != null)
            {
                groundNormal = farGround.Normal;
            }
            else
            {
                groundNormal = primaryGround.Normal;
            }

            return true;
        }

        private bool OnSteadyGround(Vector3 normal, Vector3 point)
        {
            float angle = Vector3.Angle(normal, controller.Up);

            float angleRatio = angle / groundingUpperBoundAngle;

            float distanceRatio = Mathf.Lerp(groundingMinPercentFromCenter, groundingMaxPercentFromCenter, angleRatio);

            Vector3 p = Math3D.ProjectPointOnPlane(controller.Up, controller.transform.position, point);

            float distanceFromCenter = Vector3.Distance(p, controller.transform.position);

            return distanceFromCenter <= distanceRatio * controller.radius;
        }

        private void ResetGrounds()
        {
            primaryGround = null;
            nearGround = null;
            farGround = null;
            flushGround = null;
            stepGround = null;
        }

        public Vector3 PrimaryNormal()
        {
            return primaryGround.Normal;
        }

        public float Distance()
        {
            return primaryGround.Distance;
        }

        public void DebugGround(bool primary, bool near, bool far, bool flush, bool step)
        {
            if (primary && primaryGround != null)
            {
                DebugDraw.DrawVector(primaryGround.Point, primaryGround.Normal, 2.0f, 1.0f, Color.yellow, 0, false);
            }

            if (near && nearGround != null)
            {
                DebugDraw.DrawVector(nearGround.Point, nearGround.Normal, 2.0f, 1.0f, Color.blue, 0, false);
            }

            if (far && farGround != null)
            {
                DebugDraw.DrawVector(farGround.Point, farGround.Normal, 2.0f, 1.0f, Color.red, 0, false);
            }

            if (flush && flushGround != null)
            {
                DebugDraw.DrawVector(flushGround.Point, flushGround.Normal, 2.0f, 1.0f, Color.cyan, 0, false);
            }

            if (step && stepGround != null)
            {
                DebugDraw.DrawVector(stepGround.Point, stepGround.Normal, 2.0f, 1.0f, Color.green, 0, false);
            }
        }
    }
}