public sealed class JumpEffectHandler : ICardEffectHandler
{
    public CardEffectType Type => CardEffectType.Jump;

    public bool CanAppear(CardEffect effect, CardRuntimeManager runtime) => true;

    public void Apply(CardEffect effect, CardApplyContext ctx, CardRuntimeManager runtime)
    {
        if (ctx == null || ctx.Move == null) return;
        float mul = 1f + effect.value;         // value=0.2 => 1.2น่
        if (mul <= 0f) mul = 0.01f;
        ctx.Move.ApplyJumpMultiplier(mul);
    }
}
