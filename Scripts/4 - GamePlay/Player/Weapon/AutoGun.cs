using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGun : GunBase
{
    [SerializeField] private float _damage;
    [SerializeField] private float _speed;
    [SerializeField] private float _lifeTime;

    [SerializeField] private Color _bulletColor;
    [SerializeField] private float _radius;


    private float _baseDamage;
    private float _damageMul = 1f;

    protected override void CacheBaseStats()
    {
        if(_baseCached) return;
        base.CacheBaseStats(); 
        _baseDamage = _damage;
    }

    protected override void Fire()
    {
        Vector3 dir = _muzzle.up;
        _sound.PlaySfx(SfxId.AutoGunShot);
        Util.Shoot(WeaponId,_poolName, _bulletPrefab, dir, _muzzle.position, _damage, _speed, _lifeTime, _bulletColor, _radius);
    }
    public override bool TryApplyCardEffect(CardEffect effect)
    {
        if (base.TryApplyCardEffect(effect)) return true;

        if (effect.type == CardEffectType.WeaponDamage)
        {
            CacheBaseStats();
            float mul = Mathf.Max(0.01f, 1f + effect.value);
            _damageMul *= mul;
            _damage = _baseDamage * _damageMul;
            return true;
        }
        return false;
    }



}