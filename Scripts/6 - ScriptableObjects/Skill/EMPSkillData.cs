using UnityEngine;

[CreateAssetMenu(menuName = "Skill/EMP")]
public class EMPSkillData : SkillScriptableObject
{
    [Header("EMP Range")]
    public float maxRadius;                
    public float expandDuration;           

    [Header("EMP Crowd Control")]
    public float stunDuration;            
    public bool affectBoss;                  

    [Header("Level Scaling")]
    public float[] radiusPerLevel;        
    public float[] stunPerLevel;             

    [Header("Visual")]
    public GameObject empPrefab;            


    public float GetRadius(int level) => GetValueByLevel(radiusPerLevel, level, maxRadius);
    
    public float GetStunDuration(int level) => GetValueByLevel(stunPerLevel, level, stunDuration);
    
    private float GetValueByLevel(float[] table, int level, float fallback)
    {
        if (table == null || table.Length == 0) return fallback;
        int index = level - 1;
        if (index >= table.Length) return table[^1];
        return table[index];
    }
}
