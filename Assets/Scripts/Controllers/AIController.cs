using UnityEngine;

[CreateAssetMenu(fileName = "AIController", menuName = "InputController/AIController")]
public class AIController : InputController
{
    public override bool GetJumpInput()
    {
        return true;
    }

    public override bool GetJumpRelease()
    {
        return false;
    }

    public override float GetMoveInput()
    {
        return 1f;
    }

    public override float GetVerticalInput()
    {
        return 0f;
    }
}
