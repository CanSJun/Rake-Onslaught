using System.Collections.Generic;
using UnityEngine;

public  class SkillInputManager
{
    private readonly Dictionary<KeyCode, SkillSlot> _skills;

    public SkillInputManager()
    {
        _skills = new()
        {
            { KeyCode.Z, SkillSlot.Skill1 },
            { KeyCode.X, SkillSlot.Skill2 },
            { KeyCode.C, SkillSlot.Skill3 },
        };
    }

    public bool TryRead(out SkillInput input)
    {
        foreach (var pair in _skills)
        {
            if (!Input.GetKeyDown(pair.Key))
                continue;

            input = new SkillInput(pair.Value, true);
            return true;
        }

        input = default;
        return false;
    }
}
