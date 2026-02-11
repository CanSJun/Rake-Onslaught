public sealed class SkillLevelEffectHandler : ICardEffectHandler
{
    public CardEffectType Type => CardEffectType.SkillLevel;

    public bool CanAppear(CardEffect effect, CardRuntimeManager runtime)
    {
        if (runtime != null && !string.IsNullOrEmpty(effect.targetId)) return runtime.IsSkillUnlocked(effect.targetId);
        return true;
    }

    public void Apply(CardEffect effect, CardApplyContext ctx, CardRuntimeManager runtime)
    {
        if (ctx == null || ctx.SkillManager == null) return;
        int delta = effect.intValue != 0 ? effect.intValue : 1;
        ctx.SkillManager.ApplySkillLevelUp(effect.targetId, delta);
    }
}
