using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGun : GunBase
{
    [SerializeField] private int _pelletCount = 8;
    [SerializeField] private float _spreadAngle = 10f;
    [SerializeField] private float _damage;
    [SerializeField] private float _speed;
    [SerializeField] private float _lifeTime;
    [SerializeField] private float _radius;


    [SerializeField] private Color _bulletColor;

    private float _baseDamage;
    private float _damageMul = 1f;

    protected override int GetAmmoCost() => _pelletCount;
    protected override void Start()
    {
        base.Start();

    }

    protected override void CacheBaseStats()
    {
        if (_baseCached) return;
        base.CacheBaseStats();     // 발사속도 베이스 캐시
        _baseDamage = _damage;     // 샷건 데미지 베이스 캐시
    }


    public override bool TryApplyCardEffect(CardEffect effect)
    {
        if (base.TryApplyCardEffect(effect)) return true;

        CacheBaseStats();

        if (effect.type == CardEffectType.WeaponDamage)
        {
            float mul = Mathf.Max(0.01f, 1f + effect.value);
            _damageMul *= mul;
            _damage = _baseDamage * _damageMul;
            return true;
        }

        return false;
    }

    protected override void Fire()
    {
        _sound.PlaySfx(SfxId.ShotGunShot);
        for (int i = 0; i < _pelletCount; i++)
        {
            Vector3 dir = Util.ApplySpread(_muzzle.up, _spreadAngle);
            Util.Shoot(WeaponId,_poolName, _bulletPrefab, dir, _muzzle.position, _damage, _speed, _lifeTime, _bulletColor, _radius);
        }
    }
}
