public interface IWeaponCardEffectReceiver
{
    string WeaponId { get; }
    bool TryApplyCardEffect(CardEffect effect);
}
