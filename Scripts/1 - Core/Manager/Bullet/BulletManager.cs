using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


struct BulletData
{
    public Vector3 prevPos;
    public Vector3 pos;
    public Vector3 vel;
    public float damage;
    public float radius;
    public float life;
    public string poolKey;
    public GameObject viewObj;
    public Color color;
    public string weaponId;

    // 캐싱
    public Bullet view;       
    public Transform viewTr;  
}
public class BulletManager : MonoBehaviour, IGameInitializable
{
    [Header("Refs")]
    [SerializeField] private MonsterViewManager _monsterViewManager;
    
    private PoolManager _poolManager;

    private readonly List<BulletData> _bullets = new(2048);

    public void OnGameInitialized()
    {
        _poolManager = GameManager.Instance.PoolManager;
    }


    public void Fire(Vector3 start, Vector3 dir, float speed, float damage, float lifeTime, float radius, Color color, string poolKey, string weaponId)
    {
        var obj = _poolManager.Spawn(poolKey);
        if (obj == null) return;

        var bulletView = obj.GetComponent<Bullet>();
        var tr = obj.transform;
        bulletView.SetColor(color);
        bulletView.SetPosition(start);

        BulletData data = new BulletData
        {
            prevPos = start,
            pos = start,
            vel = dir.normalized * speed,
            damage = damage,
            life = lifeTime,
            radius = radius,
            poolKey = poolKey,
            viewObj = obj,
            color = color,
            weaponId = weaponId,
            view = bulletView,
            viewTr = tr
        };

        _bullets.Add(data);
    }


    private void Update()
    {
        if (PauseManager.IsPaused) return;
        if (_bullets.Count == 0) return;

        float dt = Time.deltaTime;

        var activeMonsters = _monsterViewManager.ActiveMonster;
        for(int i = _bullets.Count - 1; i >= 0; i--)
        {
            BulletData bullet = _bullets[i];

            bullet.life -= dt;
            if(bullet.life <= 0f)
            {
                ReleaseBullet(i, bullet);
                continue;
            }

            bullet.prevPos = bullet.pos;
            bullet.pos += bullet.vel * dt;

            bullet.viewTr.position = bullet.pos;

            if (activeMonsters == null || activeMonsters.Count == 0)
            {
                _bullets[i] = bullet;
                continue;
            }

            bool hit = false;

            for(int m = 0; m < activeMonsters.Count; m++)
            {
                var monster = activeMonsters[m];
                if (monster == null || monster.IsDead || !monster.IsViewActive) continue;
                var monsterTr = monster.transform;
                Vector3 monsterCurr = monsterTr.position;
                Vector3 monsterPrev = monster.PrevPos;

                var agent = monster.GetComponent<NavMeshAgent>();
                if (agent != null && agent.enabled) monsterPrev = monsterCurr - agent.velocity * dt; // NavMeshAget로 이동하니까 Velocity로 정확도 올려서 계산

                float combinedRadius = monster.HitRadius + bullet.radius;

                if (Util.IsSweptSegmentIntersectingMovingCircleXZ(bullet.prevPos, bullet.pos,monsterPrev, monsterCurr,combinedRadius,out float t))
                {
                    Vector3 hitPoint = Vector3.Lerp(bullet.prevPos, bullet.pos, t);

                    Vector3 normal = hitPoint - monsterCurr;
                    normal.y = 0f;
                    normal = normal.sqrMagnitude > 1e-6f ? normal.normalized : Vector3.forward;

                    monster.TakeDamage(bullet.damage, hitPoint, normal, bullet.color, bullet.weaponId);

                    hit = true;
                    break;
                }
            }

            if (hit)
            {
                ReleaseBullet(i, bullet);
                continue;
            }

            _bullets[i] = bullet;
        }
    }


    private void ReleaseBullet(int index, BulletData bullet)
    {
        if(bullet.viewObj != null) _poolManager.Release(bullet.viewObj);

        int last = _bullets.Count - 1;
        _bullets[index] = _bullets[last];
        _bullets.RemoveAt(last);
    }
}