using UnityEngine;

[RequireComponent (typeof(CollisionController))]
public class Player : MonoBehaviour
{
    [SerializeField] private InputController inputController = null;

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
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
    float maxJumpVelocity;
    float minJumpVelocity;
    float velocityXSmoothing;
    float timeToWallUnStick;

    bool wallSliding;
    int wallDirX;

    Vector2 velocity;
    Vector2 input;

    CollisionController controller;

    private void Start()
    {
        controller = GetComponent<CollisionController>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print($"Gravity: {gravity} \n\rJump Velocity: {maxJumpVelocity}");
    }

    private void Update()
    {
        input = new Vector2(inputController.GetMoveInput(),inputController.GetVerticalInput());
        
        if (inputController.GetJumpInput())
            OnJumpInputDown();
        if (inputController.GetJumpRelease())
            OnJumpInputUp();

        CalculateVelocity();
        HandleWallSliding();

        controller.Move(velocity * Time.deltaTime, input);

        if (controller.collisions.above || controller.collisions.below)
        {
            if (!controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
    }

    private void CalculateVelocity()
    {
        var targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x,
            targetVelocityX,
            ref velocityXSmoothing,
            (controller.collisions.below ? acclerationTimeGround : acclerationTimeAirborne));
        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
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
    }

    private void OnJumpInputDown() //TODO:refactor into jump script
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
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (input.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) //not jumping against max slope
                {
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
            
        }
    }

    private void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }
}
