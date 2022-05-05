using UnityEngine;

public class CollisionController : RaycastController
{
    public float maxSlopeAngle = 80;

	public CollisionInfo collisions;
	private Vector2 playerInput;

    public override void Start()
    {
		collider = GetComponent<BoxCollider2D>();
		collisions.faceDirection = 1;
		base.Start();
    }

	public void Move(Vector2 moveAmount, bool standingOnPlatform)
    {
        Move(moveAmount, Vector2.zero, standingOnPlatform);
    }

    public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false) //TODO:refactor this into it's own class
    {
		UpdateRaycastOrigins();
		collisions.Reset();
		collisions.moveAmountOld = moveAmount;
		playerInput = input;        

        if (moveAmount.y < 0)
        {
            DescendSlope(ref moveAmount);
        }

		if (moveAmount.x != 0)
		{
			collisions.faceDirection = (int)Mathf.Sign(moveAmount.x);
		}

		HorizontalCollisions(ref moveAmount);

		if (moveAmount.y != 0)
        {
			VerticalCollisions(ref moveAmount);
		}        

		transform.Translate(moveAmount);

        if (standingOnPlatform)
        {
			collisions.below = true;
        }
    }	

    private void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
    {
		var moveDistance = Mathf.Abs(moveAmount.x);
		var climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (moveAmount.y <= climbmoveAmountY)
        {
			moveAmount.y = climbmoveAmountY;
			moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
			collisions.slopeNormal = slopeNormal;
		}

    }

	private void DescendSlope(ref Vector2 moveAmount)
    {
		var maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
		var maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);

        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
			SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
			SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
		}		

        if (!collisions.slidingDownMaxSlope)
        {
			var directionX = Mathf.Sign(moveAmount.x);
			var rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
			var hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

			if (hit)
			{
				var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
				{
					if (Mathf.Sign(hit.normal.x) == directionX)
					{
						if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
						{
							var moveDistance = Mathf.Abs(moveAmount.x);
							var descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
							moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
							moveAmount.y -= descendmoveAmountY;

							collisions.slopeAngle = slopeAngle;
							collisions.descendingSlope = true;
							collisions.below = true;
							collisions.slopeNormal = hit.normal;
						}
					}
				}
			}
		}		
    }

	void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {
        if (hit)
        {
			var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
				moveAmount.x = hit.normal.x * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

				collisions.slopeAngle = slopeAngle;
				collisions.slidingDownMaxSlope = true;
				collisions.slopeNormal = hit.normal;
            }
		}
    }

	void HorizontalCollisions(ref Vector2 moveAmount)
	{
		var directionX = collisions.faceDirection; ;
		var rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        if (Mathf.Abs(moveAmount.x) < skinWidth)
        {
			rayLength = 2 * skinWidth;
        }

		for (int i = 0; i < horizontalRayCount; i++)
		{
			var rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

			if (hit)
			{
                if (hit.distance == 0)
                {
					continue;
                }

				var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (i == 0 && slopeAngle <= maxSlopeAngle)
				{
					var distanceToSlopeStart = 0f;
					if (slopeAngle != collisions.slopeAngleOld)
					{
						if (collisions.descendingSlope)
						{
							collisions.descendingSlope = false;
							moveAmount = collisions.moveAmountOld;
						}
						distanceToSlopeStart = hit.distance - skinWidth;
						moveAmount.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
					moveAmount.x += distanceToSlopeStart * directionX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
				{
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					if (collisions.climbingSlope)
					{
						moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
					}

					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}

	private void VerticalCollisions(ref Vector2 moveAmount)
    {
		var directionY = Mathf.Sign(moveAmount.y);
		var rayLength = Mathf.Abs(moveAmount.y) + skinWidth;
		
		for (int i = 0; i < verticalRayCount; i++)
		{
			var rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
			var hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			
			Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if (hit)
            {
                if (hit.collider.CompareTag("MoveThrough"))
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
						continue;
                    }
                    if (hit.collider == collisions.fallingThroughPlatform)
                    {
						continue;
                    }
                    if (playerInput.y == -1)
                    {
						collisions.fallingThroughPlatform = hit.collider;
						continue;
                    }
                }
				collisions.fallingThroughPlatform = null;
				moveAmount.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
					moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
                }

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}           

		}

		if (collisions.climbingSlope)
		{
			var directionX = Mathf.Sign(moveAmount.x);
			rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
			var rayOrigin = ((directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y);
			var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
				var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
					collisions.slopeNormal = hit.normal;
				}
            }
		}
	}

	public struct CollisionInfo
    {
		public bool above, below;
		public bool left, right;
		public bool climbingSlope, descendingSlope;
		public bool slidingDownMaxSlope;

		public float slopeAngle, slopeAngleOld;
		public Vector2 slopeNormal;
		public Vector2 moveAmountOld;
		public int faceDirection;
		public Collider2D fallingThroughPlatform;

		public void Reset()
        {
			above = below = false;
            left = right = false;
			climbingSlope = false;
			descendingSlope = false;
			slidingDownMaxSlope = false;
			slopeNormal = Vector2.zero;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
        }
    }
}
