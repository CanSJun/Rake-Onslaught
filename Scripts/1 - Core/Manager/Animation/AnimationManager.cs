using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HighDefinition.CameraSettings;

public class AnimationManager
{




    public static readonly int SPEED = Animator.StringToHash("Speed");
    public static readonly int GROUNDED = Animator.StringToHash("IsGround");
    public static readonly int JUMP = Animator.StringToHash("Jump");
    public static readonly int FALLING = Animator.StringToHash("Falling");
    public static readonly int AIM = Animator.StringToHash("IsAim");
    public static readonly int X = Animator.StringToHash("X");
    public static readonly int Y = Animator.StringToHash("Y");
    public static readonly int HIT = Animator.StringToHash("Hit");
    public static readonly int STUN = Animator.StringToHash("IsStun");
    public static readonly int DIE = Animator.StringToHash("Die");
    public static readonly int Reload = Animator.StringToHash("Reload");



    public void UpdateAnimationForPlayer(Animator animator, float speed, bool isGrounded, float velocityY, Vector2 input)
    {
        animator.SetFloat(SPEED, speed);
        animator.SetBool(GROUNDED, isGrounded);
        animator.SetFloat(FALLING, velocityY);
        animator.SetFloat(X, input.x);
        animator.SetFloat(Y, input.y);
    

    }


    public void SetFloat(Animator animator, int hash, float value) => animator.SetFloat(hash, value);
    public void SetTrigger(Animator animator, int hash) => animator.SetTrigger(hash);
    public void SetBool(Animator animator, int hash, bool value) => animator.SetBool(hash, value);


}



