using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Missile")]
public class MissileSkillData : SkillScriptableObject
{
    [Header("Missile")]
    public float range;
    public float speed;
    public float damage;
    public int[] missileCountPerLevel;
    public float splashRadius = 2.5f; // 스플래시 범위
    public float splashDamageMul = 0.5f; // 주변 데미지 얼마나 적게?

    [Header("Bezier")]
    public float arcHeight;
    public float randomOffset;



    public int GetMissileCount(int level)
    {
        int index = level - 1;
        int length = missileCountPerLevel.Length;
        if (index >= length) return missileCountPerLevel[length - 1];

        return missileCountPerLevel[index];
    }
}
