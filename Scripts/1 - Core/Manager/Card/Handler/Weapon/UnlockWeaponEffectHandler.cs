public sealed class UnlockWeaponEffectHandler : ICardEffectHandler
{
    public CardEffectType Type => CardEffectType.UnlockWeapon;

    public bool CanAppear(CardEffect effect, CardRuntimeManager runtime)
    {
        if (runtime == null) return true;
        if (string.IsNullOrEmpty(effect.targetId)) return false;
        return !runtime.IsWeaponUnlocked(effect.targetId);
    }

    public void Apply(CardEffect effect, CardApplyContext ctx, CardRuntimeManager runtime)
    {
        if (runtime == null) return;
        if (string.IsNullOrEmpty(effect.targetId)) return;
        runtime.UnlockWeapon(effect.targetId);
    }
}
