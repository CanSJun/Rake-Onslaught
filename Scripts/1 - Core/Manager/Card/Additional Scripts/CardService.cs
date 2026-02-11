public class CardService
{
    private readonly CardRuntimeManager _runtime;
    private readonly CardEffectRegistry _registry;


    public CardService(CardRuntimeManager runtime, CardEffectRegistry registry)
    {
        _runtime = runtime;
        _registry = registry;
    }

    public bool CanAppear(CardData card)
    {
        if (card == null) return false;
        if (!_runtime.CanLevelUp(card)) return false;

        if (card.requiredCardIds != null)
        {
            foreach (var require in card.requiredCardIds)
            {
                if (string.IsNullOrEmpty(require)) continue;
                if (!_runtime.HasCard(require)) return false;
            }
        }

        if (card.effects != null)
        {
            foreach (var e in card.effects)
            {
                if (!_registry.TryGet(e.type, out var handler) || handler == null) return false;
                if (!handler.CanAppear(e, _runtime)) return false;
            }
        }
        return true;
    }

    public void ApplyCard(CardData card, CardApplyContext ctx)
    {
        if (card == null) return;
        if (!_runtime.CanLevelUp(card)) return;

        _runtime.AddCardLevel(card.cardId);

        if (card.effects == null) return;
        foreach (var e in card.effects)
        {
            if (_registry.TryGet(e.type, out var handler) && handler != null)  handler.Apply(e, ctx, _runtime);
        }
    }
}
