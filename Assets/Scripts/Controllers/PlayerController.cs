using UnityEngine;

[CreateAssetMenu(fileName = "PlayerController", menuName ="InputController/PlayerController")]
public class PlayerController : InputController
{
    public override bool GetJumpInput()
    {
        return Input.GetButtonDown("Jump");
    }

    public override bool GetJumpRelease()
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
