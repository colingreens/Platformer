using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputController : ScriptableObject
{
    public abstract float GetMoveInput();

    public abstract float GetVerticalInput();

    public abstract bool GetJumpInput();

    public abstract bool GetJumpRelease();
}
