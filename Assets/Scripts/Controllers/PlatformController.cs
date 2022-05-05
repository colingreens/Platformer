using UnityEngine;
using System.Collections.Generic;

public class PlatformController : RaycastController
{    
    public float speed;
    public bool cyclic;
    public float waitTime;
    [Range(0,2)]
    public float easeAmount;
    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    public LayerMask passengerMask;

    int fromWayPointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;

    List<PassengerMovement> passengerMovement;
    readonly Dictionary<Transform, CollisionController> passengerDictionary = new Dictionary<Transform, CollisionController>();
    
    public override void Start()
    {       
        collider = GetComponent<BoxCollider2D>();
        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }

        base.Start();
    }

    private void Update()
    {
        UpdateRaycastOrigins();
        var velocity = CalculatePlatformMovement();
        CalculatePassengerMovement(velocity);

        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }

    private float Ease(float x)
    {
        var a = easeAmount + 1f;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    private Vector3 CalculatePlatformMovement()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector2.zero;
        }
        
        fromWayPointIndex %= globalWaypoints.Length;
        var toWaypointIndex = (fromWayPointIndex + 1) % globalWaypoints.Length;
        var distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWayPointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed/distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        var easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        var newPos = Vector3.Lerp(globalWaypoints[fromWayPointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWayPointIndex++;

            if (!cyclic)
            {
                if (fromWayPointIndex >= globalWaypoints.Length - 1)
                {
                    fromWayPointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }

    private void MovePassengers(bool beforeMovePlatform)
    {
        foreach (var passenger in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(passenger.Transform))
            {
                passengerDictionary.Add(passenger.Transform, passenger.Transform.GetComponent<CollisionController>());
            }

            if (passenger.MoveBeforePlatform == beforeMovePlatform)
            {
                passengerDictionary[passenger.Transform].Move(passenger.Velocity, passenger.StandingOnPlatform);
            }
        }
    }

    private void CalculatePassengerMovement(Vector2 velocity)
    {
        var movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        var directionX = Mathf.Sign(velocity.x);
        var directionY = Mathf.Sign(velocity.y);

        //vertically moving platform
        if (velocity.y != 0)
        {
            var rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                var rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                var hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        var pushY = velocity.y - (hit.distance - skinWidth) * directionY;
                        var pushX = (directionY == 1) ? velocity.x : 0f;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), directionY == 1, true));                   
                    }
                }
            }
        }
        //horizontally moving platform
        if (velocity.x != 0)
        {
            var rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                var rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        var pushY = -skinWidth;
                        var pushX = velocity.x - (hit.distance - skinWidth) * directionX;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), false, true));
                    }
                }
            }
        }

        //Passenger on top of a horizontally or downward moving platform
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            var rayLength = skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++)
            {
                var rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                var hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        var pushY = velocity.y;
                        var pushX = velocity.x;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            var size = .3f;
            for (int i = 0; i < localWaypoints.Length; i++)
            {
                var globalWaypointPos = (Application.isPlaying)? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }

    struct PassengerMovement
    {
        public PassengerMovement(Transform transform, Vector2 velocity, bool standingOnPlatform, bool moveBeforePlatform)
        {
            Transform = transform;
            Velocity = velocity;
            StandingOnPlatform = standingOnPlatform;
            MoveBeforePlatform = moveBeforePlatform;
        }

        public Transform Transform;
        public Vector2 Velocity;
        public bool StandingOnPlatform;
        public bool MoveBeforePlatform;
    }
}
