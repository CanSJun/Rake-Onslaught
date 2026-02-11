using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInput
{
    Vector2 MoveInput { get; }
    bool IsRun { get; }
    bool IsJump { get; }

    bool IsAim { get; }

    bool IsShoot { get; }

    bool LeftButtonUp { get; }
    bool LeftButtonDown { get; }

    bool ReloadDown { get; }

    public Vector3 MousePosition { get; }
}

