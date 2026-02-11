using ChocDino.UIFX.Demos;

public class EMPSkillExecutor : ISkill
{
    public void Execute(in SkillContext skillStruct)
    {
        var data = (EMPSkillData)skillStruct.Skill;
        var emp = SkillUtil.Spawn(data.empPrefab, skillStruct.Caster.position);
        emp.GetComponent<EMPscript>().Initialize(data, skillStruct.Level);
    }
}
