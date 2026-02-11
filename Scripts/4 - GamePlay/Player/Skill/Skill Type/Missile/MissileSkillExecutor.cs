
public  class MissileSkillExecutor : ISkill
{
    public void Execute(in SkillContext skillStruct)
    {
        var skill = (MissileSkillData)skillStruct.Skill;
        var caster = skillStruct.Caster;



        int count = skill.GetMissileCount(skillStruct.Level);


        for (int i = 0; i < count; i++)
        {
            var missile = SkillUtil.MissileSpawn(caster.position);
            missile.Initialize(skill, caster.forward);
        }
    }
}
