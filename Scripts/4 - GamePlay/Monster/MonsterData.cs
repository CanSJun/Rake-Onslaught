using UnityEngine;

public struct MonsterData
{
    public int id;
    public float hp;
    public float maxHp;
    public float damage;
    public float speed;
    public Vector3 pos;
    public Quaternion rot;

    public bool hasView;
    public GameObject viewObj;

    public float stunRemain;

    public string poolKey;


    public float navMoveSpeed;
    public float nextNavTickTime;

    public Vector3 prevPos;

    public bool isElite;
    public float scaleMult;
}