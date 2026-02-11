using System.Collections.Generic;
using UnityEngine;

public  class SkillSystem
{
    private SkillManager _manager;

    public SkillManager Manager => _manager;
    public void Initialize(IInput input,Transform caster,Dictionary<SkillSlot, SkillScriptableObject> skillData)
    {
        var registry = new SkillRegistry(new Dictionary<SkillSlot, ISkill>
        {
            { SkillSlot.Skill1, new MissileSkillExecutor() },
            { SkillSlot.Skill2, new EMPSkillExecutor() },
            { SkillSlot.Skill3, new OverDriveSkillExecutor() }
        });
        _manager = new SkillManager( new SkillInputManager(),registry,skillData,caster);
    }

    public void Tick()
    {
        _manager.Tick();
    }
}
