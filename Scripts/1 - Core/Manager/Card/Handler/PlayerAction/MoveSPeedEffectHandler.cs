public class MoveSpeedEffectHandler : ICardEffectHandler
{
    public CardEffectType Type => CardEffectType.MoveSpeed;

    public bool CanAppear(CardEffect effect, CardRuntimeManager runtime) => true;

    public void Apply(CardEffect effect, CardApplyContext ctx, CardRuntimeManager runtime)
    {
        if (ctx == null || ctx.Move == null) return;
        float mul = 1f + effect.value;      
        if (mul <= 0f) mul = 0.01f;
        ctx.Move.ApplyMultiplierSpeed(mul);
    }
}
