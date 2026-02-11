using UnityEngine;

[CreateAssetMenu(menuName = "Config/Player Move")]
public  class MoveData : ScriptableObject
{
    [Header("Speed")]
    public float walkF;
    public float walkB;
    public float walkLR;
    public float run;
    [Header("Smooth")]
    public float rotationSmooth;
    public float speedSmooth;
    [Header("Physics")]
    public float gravity;
    public float jumpHeight;
}
