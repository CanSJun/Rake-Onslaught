
using UnityEngine;

public readonly struct SkillContext
{
    public readonly SkillScriptableObject Skill;
    public readonly Transform Caster;
    public readonly int Level;

    public SkillContext(SkillScriptableObject skill, Transform caster, int level)
    {
        Skill = skill;
        Caster = caster;
        Level = level;
    }

}
