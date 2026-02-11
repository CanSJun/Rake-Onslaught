using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlameGun : ContinuousGun
{
    [SerializeField] private float _dps = 30f;     
    [SerializeField] private float _range = 4.0f;   
    [SerializeField] private float _radius = 0.6f;    
    [SerializeField] private float _tickRate = 0.05f;
    [SerializeField] private LayerMask _hitMask;

    [SerializeField] private ParticleSystem _flameParticle;


    private float _nextTickTime;
    private MonsterViewManager _monsterViewManager;


    private float _baseDps;
    private float _baseRange;
    private float _dpsMul = 1f;
    private float _rangeMul = 1f;

    [SerializeField] private AudioSource _loopSrc;
    [SerializeField] private AudioClip _flameLoop;
    protected override void Start()
    {
        base.Start();
        _flameParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _monsterViewManager = GameManager.Instance.MonsterViewManager;

        CacheBaseStats();
    }
    protected override float GetAutoMagIntervalSeconds() => Mathf.Max(0.01f, _tickRate);

    public override void StartFire()
    {
        base.StartFire();
        if (!HasAmmo)
        {
            RequestReload();
            return;
        }

        if (_flameParticle && !_flameParticle.isPlaying) _flameParticle.Play();
        _nextTickTime = Time.time;

        _loopSrc.clip = _flameLoop;
        _loopSrc.loop = true;
        _loopSrc.spatialBlend = 0f; 
        _loopSrc.Play();
    }
    public override void StopFire()
    {
        base.StopFire();
        _loopSrc.Stop();
        if (_flameParticle) _flameParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    protected override void CacheBaseStats()
    {
        if (_baseCached) return;
        base.CacheBaseStats();  
        _baseDps = _dps;
        _baseRange = _range;
    }




    protected override void Fire(){ }
    public override void Attack()
    {
        if (!_isFiring) return;
        if (IsReloading) return;
        if (Time.time < _nextTickTime) return;
        _nextTickTime = Time.time + Mathf.Max(0.01f, _tickRate);
        if (!TryConsumeAmmo(GetAmmoCost())) return; // tick마다 연료소비
        ApplyFlameDamageTick();
    }

    protected override void OnReloadStarted()
    {
        if (_flameParticle && _flameParticle.isPlaying) _flameParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
    protected override void OnReloadFinished()
    {
        if (_isFiring)
        {
            if (_flameParticle && !_flameParticle.isPlaying) _flameParticle.Play();
            _nextTickTime = Time.time;
        }
    }

    private void ApplyFlameDamageTick()
    {
        if (_muzzle == null) return;

        var active = _monsterViewManager.ActiveMonster;
        if(active == null || active.Count == 0) return;

        Vector3 start = _muzzle.position;
        Vector3 end = start + _muzzle.up * _range;

        float tickDam = _dps * _tickRate;
        for (int i = 0; i < active.Count; i++)
        {
            var monster = active[i];
            var monsterTr = monster.transform;
            if(monster == null) continue;

            Vector3 monsterPos = monsterTr.position;
            float combined = _radius * monster.HitRadius;

            if(Util.IsLineSegmentIntersectingCircleXZ(start, end, monsterPos, combined, out _))
            {
                monster.TakeContinuousDamage(tickDam, weaponId:WeaponId);
            }
        }
    }
    public override bool TryApplyCardEffect(CardEffect effect)
    {
        if (base.TryApplyCardEffect(effect)) return true;

        CacheBaseStats();

        if (effect.type == CardEffectType.FlameDPS)
        {
            float mul = Mathf.Max(0.01f, 1f + effect.value);
            _dpsMul *= mul;
            _dps = _baseDps * _dpsMul;
            return true;
        }

        if (effect.type == CardEffectType.FlameRange)
        {
            float mul = Mathf.Max(0.01f, 1f + effect.value);
            _rangeMul *= mul;
            _range = _baseRange * _rangeMul;
            return true;
        }

        return false;
    }

}
