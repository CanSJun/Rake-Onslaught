public class ExpGainEffectHandler : ICardEffectHandler
{
    public CardEffectType Type => CardEffectType.ExpGain;

    public bool CanAppear(CardEffect effect, CardRuntimeManager runtime) => true;

    public void Apply(CardEffect effect, CardApplyContext ctx, CardRuntimeManager runtime)
    {
        if (ctx == null) return;
        var exp = ctx.PlayerExpManager;
        if (exp == null) return;

        float mul = 1f + effect.value; // value=0.25 => 경험치 1.25배
        if (mul <= 0f) mul = 0.01f;

        exp.ApplyExpMultiplier(mul);
    }
}
