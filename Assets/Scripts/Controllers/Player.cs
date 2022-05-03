using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(RaycastController))]
public class Player : MonoBehaviour
{
    [SerializeField] private InputController inputController = null;

    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;
    public float moveSpeed = 6;
    public float acclerationTimeAirborne = .2f;
    public float acclerationTimeGround = .1f;

    float gravity;
    float jumpVelocity;
    float velocityXSmoothing;

    Vector3 velocity;

    RaycastController controller;

    private void Start()
    {
        controller = GetComponent<RaycastController>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        print($"Gravity: {gravity} \n\rJump Velocity: {jumpVelocity}");
    }

    private void Update()
    {
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        if (inputController.GetJumpInput() && controller.collisions.below)
        {
            velocity.y = jumpVelocity;
        }

        var input = new Vector2(inputController.GetMoveInput(),
            inputController.GetVerticalInput());

        var targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x,
            targetVelocityX,
            ref velocityXSmoothing, (controller.collisions.below ? acclerationTimeGround : acclerationTimeAirborne));
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
