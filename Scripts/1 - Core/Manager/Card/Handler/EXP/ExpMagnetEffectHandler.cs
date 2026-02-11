public class ExpMagnetEffectHandler : ICardEffectHandler
{
    public CardEffectType Type => CardEffectType.ExpMagnet;

    public bool CanAppear(CardEffect effect, CardRuntimeManager runtime) => true;

    public void Apply(CardEffect effect, CardApplyContext ctx, CardRuntimeManager runtime)
    {
        if (ctx == null) return;
        var mgr = ctx.ExpDropManager;
        if (mgr == null) return;

        float mul = 1f + effect.value; // value=0.3 => 자석범위 1.3배
        if (mul <= 0f) mul = 0.01f;

        mgr.ApplyMagnetRadiusMultiplier(mul);
    }
}
