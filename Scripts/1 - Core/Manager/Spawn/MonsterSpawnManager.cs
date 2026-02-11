
using UnityEngine;
using UnityEngine.AI;
public class MonsterSpawnManager : MonoBehaviour, IGameInitializable
{
    [Header("Refs")]
    [SerializeField] private MonsterViewManager _monsterViewManager;
    [SerializeField] private Transform _player;
    [SerializeField] private float _viewPadding = 0.05f;

    [Header("Spawn Option")]

    [SerializeField, Tooltip("소환 이터벌")] private float _spawnInterval = 2.25f;
    [SerializeField, Tooltip("최대 소환 수")] private float _maxMonsterCount = 10000;
    [SerializeField, Tooltip("소환 반경")] private float _radius = 35f;
    [SerializeField, Tooltip("최소 소환 반경")] private float _minRadius = 20f;
    [SerializeField, Tooltip("엘리트 확률"), Range(0f, 1f)] private float _eliteChance = 0.03f;
    [SerializeField, Tooltip("엘리트 체력 배수")] private float _eliteHpMult = 2f;
    [SerializeField, Tooltip("엘리트 공격력 배수")] private float _eliteDmgMult = 2f;
    [SerializeField, Tooltip("엘리트 크기")] private float _eliteScaleMult = 3f;

    private float _nextSpawnTime;
    private int _nextId;


    private bool _ready = false;
    private Camera _camera;

    private GameManager _gameManager;

    private void Awake()
    {
        _camera = Camera.main; 
    }
    public void OnGameInitialized()
    {
        _ready = true;
        _nextSpawnTime = Time.time + 1f;
        _gameManager = GameManager.Instance;


    }


    private void Update()
    {
        if (PauseManager.IsPaused) return;
        if (_ready == false) return;
        if (Time.time < _nextSpawnTime) return;
        if (_monsterViewManager.AliveCount >= _maxMonsterCount) return;

        _nextSpawnTime = Time.time + _spawnInterval;

        OnSpawn();
    }



    private bool IsInsideCameraView(Vector3 pos)
    {
        pos.y = _camera.transform.position.y -1f; // 만약에 지면일떄 이상하게 판정이 됨으로 
        
        Vector3 vec = _camera.WorldToViewportPoint(pos);
        if (vec.z < 0f) return false; // 카메라 뒷쪽

        // 화면이 살짝 나가도 보인다고 판단! (여유 범위 포함)
        return (vec.x >= -_viewPadding && vec.x <= 1f + _viewPadding && vec.y >= -_viewPadding && vec.y <= 1f + _viewPadding);
    }

    private Vector3 GetSpawnPosition()
    {
        for(int i = 0; i < 8; i++) // 딱 8번만 찾게, 너무 찾으면 영원히 못찾으니까.. 
        {
            Vector3 pos = Util.RandomCircle(_player.position, _radius, _minRadius);

            if (!IsInsideCameraView(pos))
            {
                if (NavMesh.SamplePosition(pos, out var hit, 3f, NavMesh.AllAreas)) return hit.position;
            }
        }

        //찾은게 없다 차선책! 
        Vector3 fallback = Util.RandomCircle(_player.position, _radius, _minRadius); 
        if (NavMesh.SamplePosition(fallback, out var hit2, 3f, NavMesh.AllAreas)) return hit2.position;

        return _player.position; // 음.. 그냥 유저 위에!
    }

    private void OnSpawn()
    {

        Vector3 spawnPos = GetSpawnPosition();

        bool isElite = Random.value < _eliteChance;

        float baseHp = 30f;
        float scaledHp = baseHp * _gameManager.HpMult * ( isElite ? _eliteHpMult : 1f);

        float baseSpeed = Random.Range(2.5f, 3.5f);
        float scaledSpeed = baseSpeed * _gameManager.SpeedMult;

        float baseDamage = 0.3f;
        float scaledDamage = baseDamage * _gameManager.DamageMult * (isElite ? _eliteDmgMult : 1f);

        MonsterData data = new MonsterData
        {
            id = _nextId++,
            pos = spawnPos,
            rot = Quaternion.identity,
            hp = scaledHp,
            maxHp = scaledHp,
            speed = scaledSpeed,
            damage = scaledDamage,
            prevPos = spawnPos,

            poolKey = PoolKeys.Rake,
            hasView = false,
            viewObj = null,

            navMoveSpeed = Random.Range(2.5f, 3.5f),
            nextNavTickTime = 0f,

            isElite = isElite,
            scaleMult = isElite ? _eliteScaleMult : 1f

        };
        _monsterViewManager.AddMonster(data);
    }
}