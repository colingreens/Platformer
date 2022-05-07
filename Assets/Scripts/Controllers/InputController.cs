using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputController : ScriptableObject
{
    public abstract float GetMoveInput();

    public abstract float GetVerticalInput();

    public abstract bool GetJumpDown();

    public abstract bool GetJumpUp();
}
