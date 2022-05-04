using UnityEngine;
using System.Collections.Generic;

public class PlatformController : RaycastController
{
    public LayerMask passengerMask;
    public Vector3 move;
    
    public override void Start()
    {
        base.Start();
        collider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        UpdateRaycastOrigins();
        var velocity = move * Time.deltaTime;
        MovePassengers(velocity);
        transform.Translate(velocity);
    }

    private void MovePassengers(Vector3 velocity)
    {
        var movedPassengers = new HashSet<Transform>();

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

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        var pushY = velocity.y - (hit.distance - skinWidth) * directionY;
                        var pushX = (directionY == 1) ? velocity.x : 0f;

                        hit.transform.Translate(new Vector3(pushX, pushY));
                        movedPassengers.Add(hit.transform);
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

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        var pushY = 0f;
                        var pushX = velocity.x - (hit.distance - skinWidth) * directionX;

                        hit.transform.Translate(new Vector3(pushX, pushY));
                        movedPassengers.Add(hit.transform);
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

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        var pushY = velocity.y;
                        var pushX = velocity.x;

                        hit.transform.Translate(new Vector3(pushX, pushY));
                        movedPassengers.Add(hit.transform);
                    }
                }
            }
        }
    }
}
