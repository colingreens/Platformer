using UnityEngine;

namespace Platformer.Controller 
{
    [CreateAssetMenu(fileName = "PlayerInput", menuName = "InputController/PlayerController")]
    public class PlayerInput : InputController
    {
        public override bool GetJumpDown()
        {
            return Input.GetButtonDown("Jump");
        }

        public override bool GetJumpUp()
        {
            return Input.GetButtonUp("Jump");
        }

        public override float GetMoveInput()
        {
            return Input.GetAxisRaw("Horizontal");
        }

        public override float GetVerticalInput()
        {
            return Input.GetAxisRaw("Vertical");
        }
    }
}
