public enum SkillSlot
{
    Skill1,
    Skill2,
    Skill3,
}

[System.Serializable]
public struct SkillSlotData
{
    public SkillSlot slot;
    public SkillScriptableObject data;
}

public readonly struct SkillInput
{
    public readonly SkillSlot Slot;
    public readonly bool IsPressed;

    public SkillInput(SkillSlot slot, bool isPressed)
    {
        Slot = slot;
        IsPressed = isPressed;
    }
}
