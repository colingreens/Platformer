using UnityEngine;

public class CollisionController : RaycastController
{
	float maxClimbAngle = 80;
	float maxDescendedAngle = 75;

	public CollisionInfo collisions;

	public void Move(Vector3 velocity)
    {
		UpdateRaycastOrigins();
		collisions.Reset();
		collisions.velocityOld = velocity;

        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }

        if (velocity.x != 0)
        {
			HorizontalCollisions(ref velocity);
		}

        if (velocity.y != 0)
        {
			VerticalCollisions(ref velocity);
		}        

		transform.Translate(velocity);
    }

	

    private void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
		var moveDistance = Mathf.Abs(velocity.x);
		var climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
        }

    }

	private void DescendSlope(ref Vector3 velocity)
    {
		var directionX = Mathf.Sign(velocity.x);
		var rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		var hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
			var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendedAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
						var moveDistance = Mathf.Abs(velocity.x);
						var descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
						velocity.y -= descendVelocityY;

						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
                }
            }
        }
    }

	void HorizontalCollisions(ref Vector3 velocity)
	{
		var directionX = Mathf.Sign(velocity.x);
		var rayLength = Mathf.Abs(velocity.x) + skinWidth;

		for (int i = 0; i < horizontalRayCount; i++)
		{
			var rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (hit)
			{
				var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (i == 0 && slopeAngle <= maxClimbAngle)
				{
					var distanceToSlopeStart = 0f;
					if (slopeAngle != collisions.slopeAngleOld)
					{
						if (collisions.descendingSlope)
						{
							collisions.descendingSlope = false;
							velocity = collisions.velocityOld;
						}
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
				{
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					if (collisions.climbingSlope)
					{
						velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}

					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}

	private void VerticalCollisions(ref Vector3 velocity)
    {
		var directionY = Mathf.Sign(velocity.y);
		var rayLength = Mathf.Abs(velocity.y) + skinWidth;
		
		for (int i = 0; i < verticalRayCount; i++)
		{
			var rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			var hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			
			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
					velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}           

		}

		if (collisions.climbingSlope)
		{
			var directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			var rayOrigin = ((directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y);
			var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
				var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
                }
            }
		}
	}

	public struct CollisionInfo
    {
		public bool above, below;
		public bool left, right;
		public bool climbingSlope, descendingSlope;

		public float slopeAngle, slopeAngleOld;
		public Vector3 velocityOld;

		public void Reset()
        {
			above = below = false;
            left = right = false;
			climbingSlope = false;
			descendingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
        }
    }
}
