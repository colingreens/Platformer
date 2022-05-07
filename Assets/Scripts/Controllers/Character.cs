using UnityEngine;

namespace Platformer.CharacterController2D
{
    [RequireComponent(typeof(CharacterController2D))]
    public class Character : MonoBehaviour
    
    {
		public InputController input;

        // movement config
        
        public float runSpeed = 8f;
        public float groundAcceleration = .4f;
        public float airAcceleration = .8f;
        public float maxJumpHeight = 4f;
        public float minJumpHeight = 1f;
        public float timeToJumpApex = 0.4f;

        public Vector2 wallJumpClimb = new Vector2(7.5f, 16f);
        public Vector2 wallJumpOff = new Vector2(8.5f, 7f);
        public Vector2 wallLeap = new Vector2(18f, 17f);

        public float wallSlideSpeedMax = 3;
        public float wallStickTime = .25f;
        float timeToWallUnstick;
        bool wallSliding;
        int wallDirX;

        private float gravity;
        private float minJumpVelocity;
        private float maxJumpVelocity;
        private float velocityXSmoothing;
        private float inputX;
        private float inputY;

        private CharacterController2D _controller;
        private Vector3 _velocity;


        private void Awake()
        {
            _controller = GetComponent<CharacterController2D>();

            // listen to some events for illustration purposes
            _controller.onControllerCollidedEvent += onControllerCollider;
            _controller.onTriggerEnterEvent += onTriggerEnterEvent;
            _controller.onTriggerExitEvent += onTriggerExitEvent;
        }

        private void Start()
        {
            gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
            maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
            minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        }
        #region Event Listeners

        void onControllerCollider(RaycastHit2D hit)
        {
            // bail out on plain old ground hits cause they arent very interesting
            if (hit.normal.y == 1f)
                return;

            // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
            //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
        }


        void onTriggerEnterEvent(Collider2D col)
        {
            Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
        }


        void onTriggerExitEvent(Collider2D col)
        {
            Debug.Log("onTriggerExitEvent: " + col.gameObject.name);
        }

		#endregion

		void Update()
        {
            inputX = input.GetMoveInput();
            inputY = input.GetVerticalInput();

            if (_controller.isGrounded)
                _velocity.y = 0;

            HandleJumpDown();
            HandleJumpUp();
            CalculateVelocity();
            HandleWallSliding();

            _controller.move(_velocity * Time.deltaTime);
            _velocity = _controller.velocity;

        }

        private void HandleJumpUp()
        {
            if (input.GetJumpUp())
            {
                if (_velocity.y > minJumpVelocity)
                {
                    _velocity.y = minJumpVelocity;
                }
            }
        }

        private void CalculateVelocity()
        {
            var smoothedMovementFactor = _controller.isGrounded ? groundAcceleration : airAcceleration;
            var targetVelocityX = inputX * runSpeed;
            _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref velocityXSmoothing, smoothedMovementFactor);
            _velocity.y += gravity * Time.deltaTime;
        }

        private void HandleJumpDown()
        {
            if (input.GetJumpDown())
            {

                if (wallSliding)
                {
                    if (wallDirX == inputX)
                    {
                        _velocity.x = -wallDirX * wallJumpClimb.x;
                        _velocity.y = wallJumpClimb.y;
                    }
                    else if (inputX == 0)
                    {
                        _velocity.x = -wallDirX * wallJumpOff.x;
                        _velocity.y = wallJumpOff.y;
                    }
                    else
                    {
                        _velocity.x = -wallDirX * wallLeap.x;
                        _velocity.y = wallLeap.y;
                    }
                }
                if (_controller.isGrounded)
                {
                    _velocity.y = maxJumpVelocity;
                }
            }
        }

        private void HandleWallSliding()
        {
            wallDirX = (_controller.collisionState.left) ? -1 : 1;
            wallSliding = false;
            if ((_controller.collisionState.left || _controller.collisionState.right) && !_controller.collisionState.below && _velocity.y < 0)
            {
                wallSliding = true;

                if (_velocity.y < -wallSlideSpeedMax)
                {
                    _velocity.y = -wallSlideSpeedMax;
                }

                if (timeToWallUnstick > 0)
                {
                    velocityXSmoothing = 0;
                    _velocity.x = 0;

                    if (inputX != wallDirX && inputX  != 0)
                    {
                        timeToWallUnstick -= Time.deltaTime;
                    }
                    else
                    {
                        timeToWallUnstick = wallStickTime;
                    }
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }

            }

        }
    }
}
