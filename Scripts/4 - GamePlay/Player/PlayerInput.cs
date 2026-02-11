using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : IInput
{
    public Vector2 MoveInput => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    public bool IsRun => Input.GetKey(KeyCode.LeftShift);
    public bool IsJump => Input.GetButtonDown("Jump");

    public bool IsAim => Input.GetMouseButton(1);

    public bool IsShoot => Input.GetMouseButton(0);
    public bool LeftButtonUp => Input.GetMouseButtonUp(0);
    public bool LeftButtonDown => Input.GetMouseButtonDown(0);

    public bool ReloadDown => Input.GetKeyDown(KeyCode.R);

    public Vector3 MousePosition => Input.mousePosition;
}
