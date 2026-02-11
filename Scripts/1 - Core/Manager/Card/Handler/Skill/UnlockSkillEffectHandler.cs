public sealed class UnlockSkillEffectHandler : ICardEffectHandler
{
    public CardEffectType Type => CardEffectType.UnlockSkill;

    public bool CanAppear(CardEffect effect, CardRuntimeManager runtime)
    {
        if (runtime == null) return true;
        if (string.IsNullOrEmpty(effect.targetId)) return false;
        return !runtime.IsSkillUnlocked(effect.targetId);
    }

    public void Apply(CardEffect effect, CardApplyContext ctx, CardRuntimeManager runtime)
    {
        if (runtime == null) return;
        if (string.IsNullOrEmpty(effect.targetId)) return;
        runtime.UnlockSkill(effect.targetId);
    }
}
