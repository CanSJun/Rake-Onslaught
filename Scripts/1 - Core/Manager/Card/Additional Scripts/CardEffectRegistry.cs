using System.Collections.Generic;

public class CardEffectRegistry
{
    private readonly Dictionary<CardEffectType, ICardEffectHandler> _handlers = new();
    public void Register(ICardEffectHandler handler) => _handlers[handler.Type] = handler;
    public bool TryGet(CardEffectType type, out ICardEffectHandler handler) => _handlers.TryGetValue(type, out handler);

    public static CardEffectRegistry CreateDefault()
    {
        var reg = new CardEffectRegistry();
        reg.Register(new WeaponStatEffectHandler(CardEffectType.WeaponDamage, requireUnlockedWeapon: true));
        reg.Register(new WeaponStatEffectHandler(CardEffectType.FireRateDown, requireUnlockedWeapon: true));
        reg.Register(new WeaponStatEffectHandler(CardEffectType.FlameDPS, requireUnlockedWeapon: true));
        reg.Register(new WeaponStatEffectHandler(CardEffectType.FlameRange, requireUnlockedWeapon: true));
        reg.Register(new WeaponStatEffectHandler(CardEffectType.LaserTick, requireUnlockedWeapon: true));
        reg.Register(new WeaponStatEffectHandler(CardEffectType.ReloadTimeDown, requireUnlockedWeapon: true));

        reg.Register(new SkillLevelEffectHandler());
        reg.Register(new MoveSpeedEffectHandler());
        reg.Register(new JumpEffectHandler());
        reg.Register(new ExpMagnetEffectHandler());
        reg.Register(new ExpGainEffectHandler());

        reg.Register(new UnlockWeaponEffectHandler());
        reg.Register(new UnlockSkillEffectHandler());


        

        return reg;
    }
}
