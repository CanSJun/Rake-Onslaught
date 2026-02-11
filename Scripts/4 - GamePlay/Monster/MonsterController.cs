using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;
using System.Threading;


public class MonsterController : MonoBehaviour
{
    [Header("Monster Settings")]
    [SerializeField] private float _maxHP;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private float _damage;
    [SerializeField] private float _textHeight;
    [SerializeField] private float _hitRadius = 0.7f;
    [SerializeField] private float _dieDisableDelay = 0.05f;

    [Tooltip("100 = 1초, 10 = 0.1초 (데미지 누적 대기 시간)"), SerializeField] private int _damageDelay;
    [Tooltip("100 = 1초, 10 = 0.1초 (맞는 모션 대기 시간)"), SerializeField] private int _hitActionDelay;




    private bool _isDead;
    private float _currentHP;

    private Animator _animator;
    private Transform _target;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    private GameManager _gameManager;
    public int MonsterId { get; private set; }

    // Hit
    private MonsterHit _monsterHit;

    [SerializeField] private float _groundYOffset = 0.01f;
    [SerializeField] private Vector2 _bloodScaleRange = new Vector2(0.7f, 2.2f);
    [SerializeField, Tooltip("목적지 갱신 Tick")] private float _destinationTick = 0.25f;

    private Collider[] _colliders;
    private NavMeshAgent _navMeshAgent;
    private float _nextDestTime;
    private AnimationManager _animManager;


    private int StunHash;

    private float _stunEndTime;
    private bool _stunAniOn;


    // 초기 상태 
    private float _baseMaxHP;
    private float _baseDamage;
    private float _baseMoveSpeed;

    private bool _baseStatInitialized;

    // 충돌 스윕용 위치 기록 ( 추정 제거하기 위해 )
    private Vector3 _prevPos;
    private Vector3 _currPos;

    public Vector3 PrevPos => _prevPos;
    public Vector3 CurrPos => _currPos;

    //ELITE
    private Vector3 _baseScale;
    private float _scaleMult = 1f;
    public float GetDamage => _damage; 
    public float MaxHP => _maxHP;
    public float HP01 => _maxHP > 0f ? _currentHP / _maxHP : 0f;

    public bool IsStunned => !_isDead && Time.time < _stunEndTime;

    
    public float HitRadius => _hitRadius * _scaleMult;
    public bool IsDead => _isDead;

    [Header("Animation Settings")]
    private CancellationTokenSource _disableToken;
    [SerializeField] private int _animLayer = 0;
    private int _hitStateHash;
    private int _dieStateHash;
    private bool _hitLocked;
    private int _hitLockVersion;
    private bool _dying;

    private SoundManager _sound;
    private void Awake()
    {
        _monsterHit =  new MonsterHit();
        _colliders = GetComponentsInChildren<Collider>(true); // 비활성도 찾아야하기 때문에
        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>(true);
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _gameManager = GameManager.Instance;
        StunHash = AnimationManager.STUN;
        _animManager = _gameManager.AnimationManager;

        _hitStateHash = AnimationManager.HIT;
        _dieStateHash = AnimationManager.DIE;
        _sound = SoundManager.Instance;
    }

    private void OnEnable()
    {


        _isDead = false;

        _animator.enabled = true;
        _animator.Rebind();    
        _animator.Update(0f);   

        _currPos = transform.position;
        _prevPos = _currPos;

        _hitLocked = false;
        _hitLockVersion++;
        _dying = false;

        _disableToken?.Cancel();
        _disableToken?.Dispose();
        _disableToken = new CancellationTokenSource();
        _monsterHit.Initialize(_animator, _skinnedMeshRenderer, _damageDelay, _disableToken.Token, _hitActionDelay);

    }
    private void OnDisable()
    {
        _disableToken?.Cancel();
        _monsterHit.CleanUP();
    }
    private void Start()
    {

        
    }

    private void Update()
    {
        if (_navMeshAgent.enabled == false) return;
        if (_target == null) return;


        // 유저 사망 시
        if (PauseManager.IsPaused)
        {
            if (!_navMeshAgent.isStopped)
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.ResetPath();
            }
            return;
        }


        if (_hitLocked || IsStunned)
        {
            if (!_navMeshAgent.isStopped)
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.ResetPath();
            }
            return;
        }

        if (_stunAniOn)
        {
            _stunAniOn = false;
            _animManager.SetBool(_animator, StunHash, false);
        }
        if (_navMeshAgent.isStopped) _navMeshAgent.isStopped = false;
        if (Time.time < _nextDestTime) return;
        _nextDestTime = Time.time + _destinationTick;
        _navMeshAgent.SetDestination(_target.position);
        
    }

    private void LateUpdate()
    {
        _prevPos = _currPos;
        _currPos = transform.position;

    }


    private void BeginHitLock()
    {
        if (_isDead || _dying || !IsViewActive) return;
        if (_animator == null) return;
        _hitLocked = true;
        _hitLockVersion++;
        int ver = _hitLockVersion;
        if (_navMeshAgent && _navMeshAgent.enabled)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
        }
        WaitHitAnimationEnd(ver).Forget();
    }

    private async UniTaskVoid WaitHitAnimationEnd(int ver)
    {
        var token = _disableToken != null ? _disableToken.Token : default;
        
        //전이를 위해 약간 기다려주기
        float enterTimeout = Time.time + 0.25f;
        bool entered = false;

        while (Time.time < enterTimeout)
        {
            if (_isDead || _dying || _animator == null) return;
            var st = _animator.GetCurrentAnimatorStateInfo(_animLayer); // 현재 상태 가져오기
            if (st.shortNameHash == _hitStateHash) // 현재 상태 비교
            {
                entered = true;
                break;
            }
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        if (entered)
        {
            // Hit 상태 끝까지 대기
            float timer = Time.time + 3.0f;
            while (Time.time < timer)
            {
                if (_isDead || _dying || _animator == null) return;
                var st = _animator.GetCurrentAnimatorStateInfo(_animLayer);

                // Hit state를 벗어나거나, 끝(1.0)까지 가면 해제
                if (st.shortNameHash != _hitStateHash) break;
                if (st.normalizedTime >= 1f) break;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        if (_hitLockVersion != ver) return; // 최신 피격만 해제
        _hitLocked = false;
    }
    public void SyncPrevToCurr()
    {
        _currPos = transform.position;
        _prevPos = _currPos;
    }
    public void ApplyStun(float duration)
    {
        if (_isDead || !IsViewActive) return;
        if (duration <= 0f) return;

        float end = Time.time + duration;
        if (end > _stunEndTime) _stunEndTime = end;

        if(_navMeshAgent && _navMeshAgent.enabled)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
        }

        if (!_stunAniOn)
        {
            _stunAniOn = true;
            _animManager.SetBool(_animator, StunHash, true);
        } 
    }
    private void SettingStatsOnes()
    {
        if (_baseStatInitialized) return;

        _baseMaxHP = _maxHP;
        _baseDamage = _damage;
        _baseMoveSpeed = _navMeshAgent.speed;

        _baseScale = transform.localScale;
        _baseStatInitialized = true;
    }


    public void CreateMonster(in MonsterData monsterData, Transform player)
    {
        SettingStatsOnes();
        MonsterId = monsterData.id;
        transform.position = monsterData.pos;
        transform.rotation = monsterData.rot;


        gameObject.SetActive(true);

        _maxHP = monsterData.maxHp;    // (아래 2번에서 MonsterData에 추가할 예정)
        _currentHP = monsterData.hp;   // 현재 체력
        _damage = monsterData.damage;  // 접촉 데미지 포함
        _navMeshAgent.speed = monsterData.speed;
        _isDead = false;
        _currentHP = monsterData.hp > 0f ? monsterData.hp : MaxHP;
        if (_navMeshAgent) { 
            if (NavMesh.SamplePosition(monsterData.pos, out var hit, 2f, NavMesh.AllAreas))
            {
                _navMeshAgent.enabled = true;
                _navMeshAgent.Warp(hit.position);

                SyncPrevToCurr();

                _navMeshAgent.speed = monsterData.navMoveSpeed;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(player.position);
                _target = player;
            }
            else _navMeshAgent.enabled = false;
        }
        if (_animator) _animator.enabled = true;
        if(_skinnedMeshRenderer) _skinnedMeshRenderer.enabled = true;
        if (_colliders != null)
        {
            foreach (var collider in _colliders)
            {
                collider.enabled = true;
            }
        }

        _stunEndTime = Time.time + Mathf.Max(0f, monsterData.stunRemain);
        if (IsStunned && _navMeshAgent && _navMeshAgent.enabled)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();

            if (!_stunAniOn)
            {
                _stunAniOn = true;

                Debug.Log($"CreateMonster: animator={_animator}, animMgr={_animManager}, nav={_navMeshAgent},");
                _animManager.SetBool(_animator, StunHash, true);
            }
        }

        float size = Mathf.Max(0.01f, monsterData.scaleMult);
        _scaleMult = size;
        transform.localScale = _baseScale * size;


    }

    public void SleepMonster(ref MonsterData monsterData)
    {
        monsterData.pos = transform.position;
        monsterData.rot = transform.rotation;

        monsterData.hp = _currentHP;
        monsterData.maxHp = _maxHP;
        monsterData.damage = _damage;
        monsterData.navMoveSpeed = _navMeshAgent.speed;
        monsterData.scaleMult = _scaleMult;
        monsterData.isElite = monsterData.scaleMult > 1.01f;
        _stunAniOn = false;
        if (_animator) _animManager.SetBool(_animator, StunHash, false);

        monsterData.stunRemain = Mathf.Max(0f, _stunEndTime - Time.time);

        if (_navMeshAgent) _navMeshAgent.enabled = false;
        if (_animator) _animator.enabled = false;
        if (_skinnedMeshRenderer) _skinnedMeshRenderer.enabled = false;
        if (_colliders != null)
        {
            foreach (var collider in _colliders)
            {
                collider.enabled = false;
            }
        }
        
        gameObject.SetActive(false);
    }



    public bool IsViewActive => gameObject.activeInHierarchy;

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, Color c, string weaponId = null)
    {
        if (_isDead || !IsViewActive) return;

        _currentHP -= damage;

        Vector3 hitPos = transform.position + Vector3.up * _textHeight;
        OnEffects(hitPoint, hitNormal, c);

        bool killed = _currentHP <= 0f;

        _gameManager.GameStats.AddDamage(weaponId, damage, killed);

        if (killed)
        {
            _monsterHit.OnDamaged(damage, hitPos, true);
            Die();
            return;
        }

        BeginHitLock();
        _monsterHit.OnDamaged(damage, hitPos, false);
    }

    public void TakeSingleDamage(float damagePerTick, string weaponId = null)
    {
        if (_isDead || !IsViewActive) return;

        _currentHP -= damagePerTick;
        Vector3 hitPos = transform.position + Vector3.up * _textHeight;

        bool killed = _currentHP <= 0f;
        _gameManager.GameStats.AddDamage(weaponId, damagePerTick, killed);

        if (killed)
        {
            _monsterHit.OnDamaged(damagePerTick, hitPos, true);
            Die();
            return;
        }
        BeginHitLock();
        _monsterHit.OnDamaged(damagePerTick, hitPos, false);
    }
    public void TakeContinuousDamage(float damagePerTick, string weaponId = null)
    {
        if (_isDead || !IsViewActive) return;

        _currentHP -= damagePerTick;
        Vector3 hitPos = transform.position + Vector3.up * _textHeight;
        bool killed = _currentHP <= 0f;
        _gameManager.GameStats.AddDamage(weaponId, damagePerTick, killed);
        if (killed)
        {
            _monsterHit.OnDamaged(damagePerTick, hitPos, true);
            Die();
            return;
        }
        BeginHitLock();
        _monsterHit.OnDamaged(damagePerTick, hitPos, false);
    }


    private void OnEffects(Vector3 hitPoint, Vector3 hitNormal, Color c)
    {

        var effect = _gameManager.PoolManager.Spawn(PoolKeys.ImpactEffect);
        effect.transform.position = hitPoint;
        effect.transform.rotation = Quaternion.LookRotation(hitNormal);
        effect.GetComponent<ImpactColorManager>().ChangeEffectColor(c);

        var blood = _gameManager.PoolManager.Spawn(PoolKeys.MonsterBlood);
        blood.transform.position = hitPoint;
        blood.transform.rotation = Quaternion.LookRotation(hitNormal);
        OnGroundEffect(hitPoint);
    }

    private void OnGroundEffect(Vector3 hitPoint)
    {

        if(Physics.Raycast(hitPoint + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 10f)) // 나중에 레이어 마스크 추가
        {
            var bloodGround = _gameManager.PoolManager.Spawn(PoolKeys.MonsterGroundBlood);
            bloodGround.transform.position = hit.point + (hit.normal * _groundYOffset);

            Quaternion fix = Quaternion.Euler(90f, 0, 0);
            Quaternion slope = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion random = Quaternion.Euler(0, Random.Range(0, 360), 0);

            bloodGround.transform.rotation = slope * random * fix;

            float scale = Random.Range(_bloodScaleRange.x, _bloodScaleRange.y);
            DecalProjector projector = bloodGround.GetComponentInChildren<DecalProjector>();
            projector.size = new Vector3(scale, scale, projector.size.z);

            _gameManager.BloodGroundManager.AddBlod(bloodGround);
        }
    }

    private void Die()
    {
        if(_isDead) return;
        _isDead = true;
        _gameManager.MonsterViewManager.RemoveActive(this);
        _gameManager.MonsterViewManager.NotifyDead(MonsterId);
        _gameManager.ExpDropManager.Drop(transform.position, 1);
        if (_navMeshAgent)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
            _navMeshAgent.enabled = false;
        }


        _animManager.SetTrigger(_animator, AnimationManager.DIE);
        _sound.PlaySfxAt(SfxId.RakeDie,transform.position,maxDist:20f);
        Disable().Forget();
    }

    private async UniTaskVoid Disable()
    {
        var token = _disableToken != null ? _disableToken.Token : default;

        try
        {
            float enterTimeout = Time.time + 1.0f;
            bool entered = false;
            while (Time.time < enterTimeout)
            {
                if (_animator == null) return;
                var cur = _animator.GetCurrentAnimatorStateInfo(_animLayer);
                if (cur.shortNameHash == _dieStateHash)
                {
                    entered = true;
                    break;
                }
                // 전이 중인데 next가 Die면 진입 중으로 인정
                if (_animator.IsInTransition(_animLayer))
                {
                    var next = _animator.GetNextAnimatorStateInfo(_animLayer);
                    if (next.shortNameHash == _dieStateHash)
                    {
                        entered = true;
                        break;
                    }
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (!entered)
            {
                //만약을 위해 들어온 애들 그냥 반환
                await UniTask.Delay(400, DelayType.DeltaTime, PlayerLoopTiming.Update, token);
                _gameManager.PoolManager.Release(gameObject);
                return;
            }

            // Die 끝까지 대기하기
            float timer = Time.time + 10.0f;
            while (Time.time < timer)
            {
                if (_animator.IsInTransition(_animLayer))
                {
                    var next = _animator.GetNextAnimatorStateInfo(_animLayer);
                    if (next.shortNameHash != _dieStateHash) break; // Die->Idle 전이 시작
                }
                var st = _animator.GetCurrentAnimatorStateInfo(_animLayer);
                if (st.shortNameHash != _dieStateHash) break;
                if (st.normalizedTime >= 1f) break;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            // 마지막 프레임으로 스냅 후 고정
            _animator.Play(_dieStateHash, _animLayer, 0.999f);
            _animator.Update(0f);
            _animator.enabled = false;

            await UniTask.Delay(600, DelayType.DeltaTime, PlayerLoopTiming.Update, token);
            _gameManager.PoolManager.Release(gameObject);
        }
        catch
        {
            // 토큰 취소 시 조용히 종료
        }
    }

}
