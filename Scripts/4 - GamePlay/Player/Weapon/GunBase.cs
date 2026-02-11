using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GunBase : MonoBehaviour, IAttack, IWeaponCardEffectReceiver, IGameInitializable
{
    [GetComponents("Muzzle")] protected Transform _muzzle;
    [SerializeField] protected GameObject _bulletPrefab;
    [SerializeField] protected float _fireRate;
    [SerializeField] protected string _poolName;
    [SerializeField] private string _weaponId;

    protected float _lastFireTime;

    protected bool _baseCached;
    private float _baseFireRate;
    private float _fireRateMul = 1f;

    protected GameManager _gameManager;

    public string WeaponId => string.IsNullOrEmpty(_weaponId) ? GetType().Name : _weaponId;

    public enum AmmoMode
    {
        Infinite = 0, // TEST용
        AutoByDuration = 1, // 지속
    }
    [Header("Ammo")]
    [SerializeField] private AmmoMode _ammoMode = AmmoMode.AutoByDuration;
    [Tooltip("AutoByDuration 모드일 때, 지속 사격 가능한 초기준으로 탄창을 자동 계산")]
    [SerializeField, Min(0f)] private float _magazineSeconds = 2.5f; 
    [SerializeField, Min(1)] private int _minMagazineSize = 1;
    [SerializeField, Min(1)] private int _maxMagazineSize = 300;
    [Tooltip("1회 소비 탄약량(일반 총기=1, 특수 무기=원하는 값)")]
    [SerializeField, Min(1)] private int _ammoCost = 1;
    [Tooltip("리로드 시간(초)")]
    [SerializeField, Min(0f)] private float _reloadTime = 1.2f;

    private bool _autoReload = true;
    private int _magazineSize; // 최대 탄
    private int _currentAmmo; // 현재 남은 탄


    private float _baseReloadTime;
    private float _reloadTimeMul = 1f;

    private bool _isReloading; // 리로드 중인지 체크
    private float _reloadStartTime;
    private float _reloadEndTime;

    private Animator _animator;
    private int ReloadHash;
    public bool IsReloading => _isReloading;

    protected SoundManager _sound;

    // 탄약이 바뀔 때 마다 UI에 알려주자! 
    public delegate void AmmoChangedHandler(GunBase gun, int currentAmmo, int maxAmmo, bool isReloading);
    //event로 안하면 엉뚱한 코드가 마음대로 호출하게 되면 큰일 남, 기존 구독 다 날리거나 이러한 현상이 있기 때문에  => 안정장치
    public event AmmoChangedHandler AmmoChanged;

    // 현재 탄약
    public virtual int CurrentAmmo() => _ammoMode == AmmoMode.Infinite ? int.MaxValue : _currentAmmo;
    // 탄창 최대
    public virtual int MaxAmmo() => _ammoMode == AmmoMode.Infinite ? int.MaxValue : _magazineSize;
    // 탄환 남아 있나
    protected bool HasAmmo => _ammoMode == AmmoMode.Infinite || _currentAmmo > 0;

    /// AutoByDuration 계산에 사용하는 시간</summary>
    protected virtual float GetAutoMagIntervalSeconds() => _fireRate;

    /// 1회당 얼마나 소비 할건가
    protected virtual int GetAmmoCost() => _ammoCost;

    protected virtual void Start()
    {
        Util.ApplyComponents(this);
        _gameManager = GameManager.Instance;
        ReloadHash = AnimationManager.Reload;
        SettingBaseCached();
        _sound = SoundManager.Instance;
    }
    public void OnGameInitialized()
    {
        _animator = _gameManager.PlayerAnimator;
        
    }
    protected void SettingBaseCached()
    {
        if (_baseCached) return;
        CacheBaseStats();     
        _baseCached = true;
        InitAmmo();
    }

    private void InitAmmo()
    {
        RecalculateMagazineSize();
        if (_ammoMode == AmmoMode.Infinite)
        {
            _currentAmmo = int.MaxValue;
            _magazineSize = int.MaxValue;
        }
        else _currentAmmo = _magazineSize;
        NotifyAmmoChanged();
    }

    // Ammo가 바뀌면 알려주기
    protected void NotifyAmmoChanged()
    {
        var handler = AmmoChanged; // 로컬 캐싱
        if (handler == null) return;
        handler(this, CurrentAmmo(), MaxAmmo(), _isReloading);
    }
    // 재계산
    protected void RecalculateMagazineSize()
    {
        if (_ammoMode == AmmoMode.Infinite)
        {
            _magazineSize = int.MaxValue;
            return;
        }

        float interval = GetAutoMagIntervalSeconds();
        if (interval <= 0f) interval = 0.01f;

        int uses = Mathf.CeilToInt(_magazineSeconds / interval);
        int size = uses * GetAmmoCost();
        _magazineSize = Mathf.Clamp(size, _minMagazineSize, _maxMagazineSize);

        if (_currentAmmo > _magazineSize) _currentAmmo = _magazineSize;
    }

    protected virtual void CacheBaseStats()
    {
        if (_baseCached) return;
        _baseFireRate = _fireRate;
        _baseReloadTime = _reloadTime;
    }
    private void Update()
    {
        // 리로드 완료 타이밍을 체크
        if (_isReloading && Time.time >= _reloadEndTime) FinishReload();
        
    }
    public virtual void Attack()
    {
        if (_isReloading) return;
        if (Time.time - _lastFireTime < _fireRate) return;
        if (!TryConsumeAmmo(GetAmmoCost())) return; // 탄알이 없음!
        _lastFireTime = Time.time;
        Fire();
    }
    protected abstract void Fire();
    public virtual void OnHold() => Attack();
    public virtual void OnDown() { }
    public virtual void OnUp() { }




    /// <summary>
    /// 탄을 소비. 부족하면(또는 0이면) 자동 리로드.
    /// Continuous 계열도 tick마다 이걸 쓰면 됨.
    /// </summary>
    protected bool TryConsumeAmmo(int amount)
    {
        if (_ammoMode == AmmoMode.Infinite) return true; // 테스트용
        if (_isReloading) return false; // 리로드 중
        if (amount <= 0) return true; // 탄약을 안쓰는 총
        if (_currentAmmo < amount)
        {
            if (_autoReload) BeginReload(); // 탄이 모자르니 자동으로 리로드
            return false;
        }
        _currentAmmo -= amount;
        NotifyAmmoChanged();
        if (_currentAmmo <= 0 && _autoReload) BeginReload();
        return true;
    }

    // 수동 리로드
    public void RequestReload()
    {
        if (_ammoMode == AmmoMode.Infinite) return;
        if (_isReloading) return;
        BeginReload();
    }

    private void BeginReload()
    {
        if (_ammoMode == AmmoMode.Infinite) return;
        if (_isReloading) return;

        _isReloading = true;
        _reloadStartTime = Time.time;
        _reloadEndTime = _reloadStartTime + _reloadTime;
        _sound.PlaySfx(SfxId.Reload);
        OnReloadStarted(); // 리로드 시작 
        NotifyAmmoChanged(); // 리로드 중 표시
    }

    private void FinishReload()
    {
        if (!_isReloading) return;

        _isReloading = false;
        RecalculateMagazineSize();     // 혹시 AutoByDuration이면 연산이나 cost가 바뀌엇을 때 탄창 크기가 달라질 수 있기 때문에 안정 장치로
        _currentAmmo = _magazineSize;  // 무조건 풀로

        OnReloadFinished(); // 리로드 끝
        NotifyAmmoChanged(); // 리로드 끝
    }

    // 나중에 사운드 연결
    protected virtual void OnReloadStarted() { }
    protected virtual void OnReloadFinished() { }


    public virtual bool TryApplyCardEffect(CardEffect effect)
    {
        switch (effect.type)
        {
            case CardEffectType.FireRateDown:
                FireRateDown(1f - effect.value);
             return true;
            case CardEffectType.ReloadTimeDown:
                ReloadTimeDown(1f - effect.value);
                return true;
        }
        return false;
    }

    protected void ReloadTimeDown(float mul)
    {
        CacheBaseStats();
        if (mul <= 0f) mul = 0.01f;
        _reloadTimeMul *= mul;
        _reloadTime = Mathf.Max(0.01f, _baseReloadTime * _reloadTimeMul);
    }

    protected void FireRateDown(float mul)
    {
        CacheBaseStats();
        if (mul <= 0f) mul = 0.01f;
        _fireRateMul *= mul;
        _fireRate = Mathf.Max(0.01f, _baseFireRate * _fireRateMul);
        RecalculateMagazineSize();
        NotifyAmmoChanged();
    }
}
