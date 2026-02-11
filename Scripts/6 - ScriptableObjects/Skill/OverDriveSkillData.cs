using UnityEngine;

[CreateAssetMenu(menuName = "Skill/OverDrive")]
public class OverDriveSkillData : SkillScriptableObject
{
    [Header("OverDrive")]
    public float duration;
    public float speedMultiplier;

    [Header("Level Scaling")]
    public float[] durationPerLevel;
    public float[] speedMultPerLevel;

    [Header("Collision Ignore")]
    public int playerLayer;
    public int enemyLayer;

    public float GetDuration(int level) => GetValueByLevel(durationPerLevel, level, duration);
    public float GetSpeedMult(int level) => GetValueByLevel(speedMultPerLevel, level, speedMultiplier);

    private float GetValueByLevel(float[] table, int level, float fallback)
    {
        if (table == null || table.Length == 0) return fallback;
        int index = level - 1;
        if (index >= table.Length) return table[^1];
        return table[index];
    }
}