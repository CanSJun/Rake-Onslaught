public class OverDriveSkillExecutor : ISkill
{
    public void Execute(in SkillContext skillStruct)
    {
        var data = (OverDriveSkillData)skillStruct.Skill;
        var player = skillStruct.Caster.GetComponent<PlayerController>();
        if (player == null) return;
        player.ActivateOverDrive(data, skillStruct.Level);
    }
}