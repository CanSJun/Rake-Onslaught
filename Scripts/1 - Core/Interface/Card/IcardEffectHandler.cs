public interface ICardEffectHandler
{
    CardEffectType Type { get; }
    bool CanAppear(CardEffect effect, CardRuntimeManager runtime);
    void Apply(CardEffect effect, CardApplyContext ctx, CardRuntimeManager runtime);
}
