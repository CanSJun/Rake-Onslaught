using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static Util;


public class PlayerController : MonoBehaviour, IGameInitializable
{
    private IInput _input;
    private MoveSystem _playerMove;
    private PlayerCombatSystem _playerCombat;

    [Header("Configs")]
    [SerializeField] private MoveData _moveData;
    [SerializeField] private GunBase _weaponRoot;
    [SerializeField] private SkillSlotData[] _skillSlots;
    [SerializeField] private float _damageTick = 0.3f;
    [SerializeField] private HPScript _hpUI;
    [SerializeField, Tooltip("제한")] private float _minX,_maxX;
    [SerializeField, Tooltip("제한")] private float _minZ,_maxZ;

    [SerializeField] private float _damageCheckInterval = 0.05f;
    private float _nextDamageCheckTime;

    private GunBase[] _guns;
    private int _currentOverDirve = 0;


    private int NomalLayer;
    private int GhostLayer;
    private CameraScript _cameraScript;
    private float _nextDamageTick;
    private GameManager _gameManager;

    private readonly Dictionary<string, int> _weaponIdToIndex = new();
    private readonly Dictionary<string, SkillSlot> _skillIdToSlot = new();
    private bool _runtimeBound;

    private CharacterController _characterController;

    private GunBase _uiGun;
    private int _uiGunIndex;

    private CardRuntimeManager _runtime;

    public MoveSystem Move => _playerMove;
    public PlayerCombatSystem Combat => _playerCombat;

    private SoundManager _sound;

    private bool _initCheck = false;
    private void Awake()
    {
        _input = new PlayerInput();
        _playerMove = new MoveSystem();
        _playerCombat = new PlayerCombatSystem();
        _sound = SoundManager.Instance;


        _guns = _weaponRoot.GetComponents<GunBase>();

        NomalLayer = gameObject.layer;
        GhostLayer = LayerMask.NameToLayer("PlayerGhost");
        UnlockLookup();
        _runtime = GameManager.Instance.CardGameManager.Runtime;

    }
    private void Start()
    {
        _gameManager = GameManager.Instance;
        _cameraScript = Camera.main.GetComponent<CameraScript>();
        _characterController = GetComponent<CharacterController>();
        
    }
    public void OnGameInitialized()
    {
        /// SKILL 초기화 ///
        var skillData = new Dictionary<SkillSlot, SkillScriptableObject>();
        foreach (var slot in _skillSlots)
        {
            skillData[slot.slot] = slot.data;
        }
        _playerMove.Initialize(GetComponent<CharacterController>(), _gameManager.PlayerAnimator, _input, transform, _moveData, _minX, _maxX, _minZ, _maxZ);
        _playerCombat.Initialize(_input, transform, _guns, skillData);
        
        BindWeaponUI(0);

        _initCheck = true;
    }
    private void Update()
    {
        if (START == 0) return;
        if (!_initCheck) return;
        if (PauseManager.IsPaused) return;
        _playerMove.Tick();
        _playerCombat.Tick();

        HandleWeaponInput();
        CheckMonsterContactDamage();
    }
    private void HandleWeaponInput()
    {
        if (!Input.anyKeyDown) return;

        string s = Input.inputString;
        if (string.IsNullOrEmpty(s)) return;

        char c = s[0];
        int index = c - '1';      // 1~4 → 0~3
        if (index < 0 || index >= 4) return;
        var gun = _guns[index];
        if (gun && !_runtime.IsWeaponUnlocked(gun.WeaponId)) return; // 해금이 안됨

        _playerCombat.ChangeWeapon(index);
        BindWeaponUI(index);
    }

    private void BindWeaponUI(int index)
    {
        var gun = _playerCombat.CurrentGun;
        if (gun == null) return;

        // 이전 무기 이벤트 구독 해제
        if (_uiGun != null) _uiGun.AmmoChanged -= OnAmmoChanged;

        _uiGun = gun;
        _uiGunIndex = index;

        // 새 무기 이벤트 구독
        _uiGun.AmmoChanged += OnAmmoChanged;

        // UI 즉시 동기화
        _gameManager.WeaponSlotManager.ChangeSlotIndex(_uiGunIndex,_uiGun.CurrentAmmo(),_uiGun.MaxAmmo(),_uiGun.IsReloading);
    }

    private void OnAmmoChanged(GunBase gun, int currentAmmo, int maxAmmo, bool isReloading)
    {
        if (gun != _uiGun) return;
        _gameManager.WeaponSlotManager.UpdateAmmo(currentAmmo, maxAmmo, isReloading);
    }
    public void ActivateOverDrive(OverDriveSkillData data, int level) => OnOverDrive(data, level).Forget();

    private async UniTaskVoid OnOverDrive(OverDriveSkillData data, int level)
    {
        var token = this.GetCancellationTokenOnDestroy();

        float duration = data.GetDuration(level);
        float mul = data.GetSpeedMult(level);

        if (!this) return; // 만약에 시작 지점에 파괴가 되어있으면 리턴

        _currentOverDirve++; // 중복 발동 체크
        if (_currentOverDirve == 1) { 
            gameObject.layer = GhostLayer;
            _cameraScript?.SetOverDriveZoom(true);
        }
        _playerMove.ApplyMultiplierSpeed(mul);
        _sound.PlaySfx(SfxId.OverDrive);
        try
        {
            await UniTask.Delay((int)(duration * 1000f), delayType:DelayType.DeltaTime, cancellationToken:token);
        }
        finally
        {
            if (this)
            {
                _playerMove.RemoveMultiplierSpeed(mul);
                _currentOverDirve = Mathf.Max(0, _currentOverDirve - 1);
                if (_currentOverDirve == 0) { 
                    gameObject.layer = NomalLayer;
                    _cameraScript?.SetOverDriveZoom(false);
                }
            }
        }
    }



    public void SetUI()
    {
        if (_gameManager == null) _gameManager = GameManager.Instance;

        BindRuntime(true);
        EnsureUnlocked();
        _playerCombat?.ChangeWeapon(0);
        BindWeaponUI(0);
        SetWeaponLocks();
        SetSkillLocks();
    }
    private void SetWeaponLocks()
    {
        if (_gameManager == null) return;
        if (_runtime == null) return;
        if (_guns == null) return;

        for (int i = 0; i < _guns.Length; i++)
        {
            var g = _guns[i];
            if (g == null) continue;
            bool locked = !_runtime.IsWeaponUnlocked(g.WeaponId);
            _gameManager.WeaponSlotManager.SetLocked(i, locked);
        }
    }
    private void SetSkillLocks()
    {
        if (_runtime == null) return;
        if (_skillSlots == null) return;

        for (int i = 0; i < _skillSlots.Length; i++)
        {
            var slot = _skillSlots[i].slot;
            var data = _skillSlots[i].data;
            if (data == null) continue;

            bool locked = !_runtime.IsSkillUnlocked(data.skillID);
            _gameManager.SkillSlotManager.SetLocked(slot, locked);
        }
    }
    private void CheckMonsterContactDamage()
    {
        if (gameObject.layer == GhostLayer) return;

        if (Time.time < _nextDamageCheckTime) return;
        _nextDamageCheckTime = Time.time + _damageCheckInterval;

        if (Time.time < _nextDamageTick) return;
        
        Vector3 playerPos = transform.position;
        foreach (var monster in _gameManager.MonsterViewManager.ActiveMonster)
        {
            if (monster.IsDead) continue;
            if(monster.IsStunned) continue;
            float hitDist = _characterController.radius + monster.HitRadius;
            float sqrDist = (monster.CurrPos - playerPos).sqrMagnitude;
            if (sqrDist <= hitDist * hitDist)
            {
                float baseDamage = monster.GetDamage;
                float damage = UnityEngine.Random.Range(baseDamage * 0.7f, baseDamage);
                _hpUI.SetHp(damage);
                _nextDamageTick = Time.time + _damageTick;
                _sound.PlaySfx(SfxId.HitSound);
                break;
            }
        }
    }

    private void BindRuntime(bool check)
    {
        if (_runtime == null && _gameManager != null && _gameManager.CardGameManager != null)
            _runtime = _gameManager.CardGameManager.Runtime;

        if (_runtime == null) return;

        if (!_runtimeBound)
        {
            _runtime.WeaponUnlocked += OnWeaponUnlocked;
            _runtime.SkillUnlocked += OnSkillUnlocked;
            _runtimeBound = true;

            if (check)
            {
                SetWeaponLocks();
                SetSkillLocks();
            }
        }
    }

    private void OnWeaponUnlocked(string weaponId)
    {
        if (string.IsNullOrEmpty(weaponId)) return;
        if (_weaponIdToIndex.TryGetValue(weaponId, out var idx)) _gameManager.WeaponSlotManager.SetLocked(idx, false);
        else SetWeaponLocks(); // 혹시나 맵핑 까졌으면 전체 갱신으로 방지!
    }

    private void OnSkillUnlocked(string skillId)
    {
        if (string.IsNullOrEmpty(skillId)) return;
        if (_skillIdToSlot.TryGetValue(skillId, out var slot)) _gameManager.SkillSlotManager.SetLocked(slot, false);
        else SetSkillLocks();
    }

    private void EnsureUnlocked()
    {
        if (_runtime == null) return;
        var gun = _playerCombat != null ? _playerCombat.CurrentGun : null;
        if (gun == null) return;
        if (!_runtime.IsWeaponUnlocked(gun.WeaponId)) _runtime.UnlockWeapon(gun.WeaponId);
    }

    private void OnDestroy()
    {
        //만약을 위해서
        if (_uiGun != null) _uiGun.AmmoChanged -= OnAmmoChanged;
    }


    private void UnlockLookup()
    {
        _weaponIdToIndex.Clear();
        if (_guns != null)
        {
            for (int i = 0; i < _guns.Length; i++)
            {
                var g = _guns[i];
                if (g == null) continue;
                var id = g.WeaponId;
                if (string.IsNullOrEmpty(id)) continue;
                _weaponIdToIndex[id] = i;
            }
        }

        _skillIdToSlot.Clear();
        if (_skillSlots != null)
        {
            for (int i = 0; i < _skillSlots.Length; i++)
            {
                var data = _skillSlots[i].data;
                if (data == null) continue;
                var id = data.skillID;
                if (string.IsNullOrEmpty(id)) continue;
                _skillIdToSlot[id] = _skillSlots[i].slot;
            }
        }
    }
}
