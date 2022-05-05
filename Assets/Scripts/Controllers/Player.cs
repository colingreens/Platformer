using UnityEngine;

[RequireComponent (typeof(CollisionController))]
public class Player : MonoBehaviour
{
    [SerializeField] private InputController inputController = null;

    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallJumpLeap;
    public float moveSpeed = 6;
    public float acclerationTimeAirborne = .2f;
    public float acclerationTimeGround = .1f;
    public float wallSlideSpeedMax = 3f;
    public float wallStickTime = .25f;

    float gravity;
    float jumpVelocity;
    float velocityXSmoothing;
    float timeToWallUnStick;

    Vector3 velocity;

    CollisionController controller;

    private void Start()
    {
        controller = GetComponent<CollisionController>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        print($"Gravity: {gravity} \n\rJump Velocity: {jumpVelocity}");
    }

    private void Update()
    {
        var input = new Vector2(inputController.GetMoveInput(),
            inputController.GetVerticalInput());
        var wallDirX = (controller.collisions.left) ? -1 : 1;

        var targetVelocityX = input.x * moveSpeed;

        velocity.x = Mathf.SmoothDamp(velocity.x,
            targetVelocityX,
            ref velocityXSmoothing,
            (controller.collisions.below ? acclerationTimeGround : acclerationTimeAirborne));

        var wallSliding = false;

        if (controller.collisions.left || controller.collisions.right
            && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;
            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnStick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (input.x != wallDirX && input.x != 0)
                {
                    timeToWallUnStick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnStick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnStick = wallStickTime;
            }
        }

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        if (inputController.GetJumpInput())
        {
            if (wallSliding)
            {
                if (wallDirX == input.x)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if (input.x == 0)
                {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else
                {
                    velocity.x = -wallDirX * wallJumpLeap.x;
                    velocity.y = wallJumpLeap.y;
                }
            }
            if (controller.collisions.below)
            {
                velocity.y = jumpVelocity;
            }
        } 

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
