public class WeaponStatEffectHandler : ICardEffectHandler
{
    public CardEffectType Type { get; }
    private readonly bool _requireUnlockedWeapon;
    public WeaponStatEffectHandler(CardEffectType type, bool requireUnlockedWeapon)
    {
        Type = type;
        _requireUnlockedWeapon = requireUnlockedWeapon;
    }
    public bool CanAppear(CardEffect effect, CardRuntimeManager runtime)
    {
        if (_requireUnlockedWeapon && runtime != null) return runtime.IsWeaponUnlocked(effect.targetId);
        return true;
    }

    public void Apply(CardEffect effect, CardApplyContext ctx, CardRuntimeManager runtime)
    {
        if (ctx == null) return; ctx.ApplyEffectToWeapons(effect.targetId, effect);
    }
}