using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGun : ContinuousGun
{

    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _maxDistance = 20f;
    [SerializeField] private float _tickInterval = 0.1f;
    [SerializeField] private float _beamWidth = 0.6f;
    [SerializeField] private Gradient _gradient;

    private LineRenderer _lineRenderer;

    private bool _isActive;
    private float _nextTick;
    private float _hitRadius;

    private float _baseDamage;
    private float _baseTickInterval;
    private float _damageMul = 1f;
    private float _tickMul = 1f;

    [SerializeField] private AudioSource _loopSrc;
    [SerializeField] private AudioClip _lagerLoop;
    protected override void Start()
    {
        base.Start();

        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;
        _lineRenderer.positionCount = 2;
        _lineRenderer.colorGradient = _gradient;
        

        _hitRadius = _beamWidth * 0.5f;
        ChangeBeamWidht(_beamWidth);
        _nextTick = Time.time;
    }
    protected override float GetAutoMagIntervalSeconds() => Mathf.Max(0.01f, _tickInterval);
    protected override void CacheBaseStats()
    {
        if (_baseCached) return;
        base.CacheBaseStats();
        _baseDamage = _damage;
        _baseTickInterval = _tickInterval;
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

        if (effect.type == CardEffectType.LaserTick)
        {
            // LaserTick: "-%" 의미(틱 간격 감소)
            float mul = Mathf.Max(0.01f, 1f - effect.value);
            _tickMul *= mul;
            _tickInterval = Mathf.Max(0.01f, _baseTickInterval * _tickMul);
            RecalculateMagazineSize();
            NotifyAmmoChanged();
            return true;
        }

        return false;
    }

    public void ChangeBeamWidht(float widht)
    {
        _beamWidth = widht;
        _hitRadius = _beamWidth * 0.5f;
        if (!_lineRenderer) return;
        _lineRenderer.startWidth = _beamWidth;
        _lineRenderer.endWidth = _beamWidth;
    }

    public override void StartFire()
    {
        base.StartFire();

        if (IsReloading) return;
        if (!HasAmmo)
        {
            RequestReload();
            return;
        }

        ActivateBeam();

    }
    public override void StopFire()
    {
        base.StopFire();
        DeactivateBeam();
    }

    public override void OnHold()
    {
        if (!_isFiring) return;

        if (IsReloading)
        {
            if (_isActive) DeactivateBeam();
            return;
        }

        if (!HasAmmo)
        {
            RequestReload();
            if (_isActive) DeactivateBeam();
            return;
        }

        if (!_isActive) ActivateBeam();
        OnBeam();
    }

    private void ActivateBeam()
    {
        _isActive = true;
        _lineRenderer.enabled = true;
        _nextTick = Time.time;
        _loopSrc.clip = _lagerLoop;
        _loopSrc.loop = true;
        _loopSrc.spatialBlend = 0f;  // 플레이어 본인 소리면 2D가 대부분 편함
        _loopSrc.Play();
    }

    private void DeactivateBeam()
    {

        _isActive = false;
        ClearBeam();
        _loopSrc.Stop();
    }

    private void ClearBeam()
    {
        _lineRenderer.enabled = false;
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, Vector3.zero);
        _lineRenderer.SetPosition(1, Vector3.zero);
    }

    private void OnBeam()
    {
        Vector3 start = _muzzle.position;
        Vector3 direction = _muzzle.up;

        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, start + direction * _maxDistance);

        if (Time.time >= _nextTick)
        {
            _nextTick = Time.time + _tickInterval;
            if (!TryConsumeAmmo(GetAmmoCost())) return;
            _gameManager.MonsterViewManager.ApplyLineDamage(start, start + direction * _maxDistance, _hitRadius, _damage, WeaponId);
        }

    }
    protected override void OnReloadStarted()
    {
        if (_isActive) DeactivateBeam();
    }

    protected override void OnReloadFinished()
    {
        if (_isFiring && !IsReloading)
        {
            ActivateBeam();
        }
    }

    private void LateUpdate()
    {
        if (!_isFiring || IsReloading)
        {
            if (_lineRenderer && _lineRenderer.enabled) ClearBeam();
        }
    }

    protected override void Fire()
    {
    }

}
