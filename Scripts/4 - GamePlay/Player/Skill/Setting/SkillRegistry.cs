using System.Collections.Generic;

public class SkillRegistry
{
    private readonly Dictionary<SkillSlot, ISkill> _skills;

    public SkillRegistry(Dictionary<SkillSlot, ISkill> skills)
    {
        _skills = skills;
    }

    public ISkill GetSkill(SkillSlot slot) => _skills[slot];
}