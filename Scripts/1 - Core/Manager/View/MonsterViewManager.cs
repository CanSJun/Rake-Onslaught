using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Util;
public class MonsterViewManager : MonoBehaviour, IGameInitializable
{
    [Header("Setting")]
    [SerializeField] private Transform _player;
    [Header("Option")]
    [SerializeField, Tooltip("화면에 유지할 최대 몬스터 수")] private int _maxView = 400;
    [SerializeField, Tooltip("몬스터 반경")] private float _radius = 0.7f;
    [SerializeField, Tooltip("보여줄 범위")] private float _showDistance = 28f;
    [SerializeField, Tooltip("숨길 범위")] private float _hideDistance = 36f;
    [SerializeField, Tooltip("숨겨진 몬스터 이동 Tick")] private float _navTick = 0.35f;

    [Header("Budget (per frame)")]
    [SerializeField] private int _hideBudgetPerFrame = 60;
    [SerializeField] private int _spawnBudgetPerFrame = 20;


    public List<MonsterData> _monsters = new();
    private Camera _camera;
    private ExpDropManager _dropManager;
    private PoolManager _poolManager;
    private CullingGroup _cullingGroup;
    private BoundingSphere[] _spheres;


    private readonly List<int> _visibleCandidates = new();
    private readonly List<int> _activeView = new();
    private readonly HashSet<int> _selectedView = new();


    //활성화 된 몬스터 등록
    private readonly List<MonsterController> _activeMonster = new(512);
    public IReadOnlyList<MonsterController> ActiveMonster => _activeMonster;

    private float[] _distanceBands;

    // 중복 예약 방지
    private readonly Queue<int> _hideQueue = new();
    private readonly Queue<int> _spawnQueue = new();
    private readonly HashSet<int> _pendingHide = new();
    private readonly HashSet<int> _pendingSpawn = new();


    public int AliveCount { get; private set; }

    public int MonsterCount => _monsters.Count;

    
    public void AddMonster(MonsterData monster) {

        _monsters.Add(monster);
        if (monster.hp > 0f) AliveCount++;
        RebuildCullingGroup();
    }
    private void RebuildCullingGroup()
    {
        SetCullingGroup();
        if (_cullingGroup == null) return;
        _spheres = new BoundingSphere[_monsters.Count];
        for (int i = 0; i < _monsters.Count; i++) _spheres[i] = new BoundingSphere(_monsters[i].pos, _radius);
        _cullingGroup.SetBoundingSpheres(_spheres);
        _cullingGroup.SetBoundingSphereCount(_spheres.Length);
    }


    public void AddMonsterImmediate(in MonsterData monster, Transform player)
    {
        // 1) 데이터 등록
        _monsters.Add(monster);
        if (monster.hp > 0f) AliveCount++;
        RebuildCullingGroup();

        // 2) viewObj가 있으면 ActiveMonster에도 등록
        if (monster.viewObj != null)
        {
            var ctrl = monster.viewObj.GetComponent<MonsterController>();
            if (ctrl != null && !_activeMonster.Contains(ctrl))
                _activeMonster.Add(ctrl);

            // MonsterController 초기화까지 여기서 확정
            if (ctrl != null)
                ctrl.CreateMonster(in monster, player);
        }
    }


    public void OnGameInitialized() {
        _poolManager = GameManager.Instance.PoolManager;
        _dropManager = GameManager.Instance.ExpDropManager;
        _camera = Camera.main;
        SetCullingGroup();
        SettingView(_monsters); 
    }

    private void SetCullingGroup()
    {
        if (_camera == null) _camera = Camera.main;
        if (_camera == null) return;

        if (_distanceBands == null || _distanceBands.Length != 2)  _distanceBands = new[] { _showDistance, _hideDistance };
        if (_cullingGroup != null) return;
        _cullingGroup = new CullingGroup();
        _cullingGroup.targetCamera = _camera;
        _cullingGroup.SetDistanceReferencePoint(_camera.transform);
        _cullingGroup.SetBoundingDistances(_distanceBands);
    }

    public void SettingView(List<MonsterData> data)
    {
        _monsters = data;
        AliveCount = 0;
        for (int i = 0; i < _monsters.Count; i++) if (_monsters[i].hp > 0f) AliveCount++;
        RebuildCullingGroup();

    }

    public void NotifyDead(int monsterId)
    {
        for (int i = 0; i < _monsters.Count; i++)
        {
            if (_monsters[i].id != monsterId) continue;
            var d = _monsters[i];
            if (d.hp > 0f) AliveCount--;
            d.hp = 0f;
            d.stunRemain = 0f;
            d.hasView = false;
            d.viewObj = null;
            _monsters[i] = d;
            return;
        }

    }
    /*    public void SettingView(List<MonsterData> data)
        {
            _monsters = data;

            _spheres = new BoundingSphere[data.Count];
            for (int i = 0; i < _monsters.Count; i++) _spheres[i] = new BoundingSphere(_monsters[i].pos, _radius);

            _distanceBands = new[] { _showDistance, _hideDistance };

            _cullingGroup?.Dispose();
            _cullingGroup = new CullingGroup();
            _cullingGroup.targetCamera = _camera;
            _cullingGroup.SetBoundingSpheres(_spheres);
            _cullingGroup.SetBoundingSphereCount(_spheres.Length);
            _cullingGroup.SetDistanceReferencePoint(_camera.transform);
            _cullingGroup.SetBoundingDistances(_distanceBands);
        }*/

    private void LateUpdate()
    {
        if (_cullingGroup == null || _spheres == null || START == 0) return;

        for (int i = 0; i < _monsters.Count; i++) _spheres[i].position = _monsters[i].pos;
        CollectCandidates();
        CreateBuild();
        ProcessQueue();

        HideMonsterTick(_player.position);
    }

    private void CollectCandidates()
    {
        _visibleCandidates.Clear();
        _activeView.Clear();

        for(int i = 0; i < _monsters.Count; i++)
        {
            if (_monsters[i].hasView) _activeView.Add(i);

            if (_cullingGroup.IsVisible(i))
            {
                int band = _cullingGroup.GetDistance(i);
                if(band <= 1) _visibleCandidates.Add(i);
            }
        }
    }



    private void CreateBuild()
    {
        var camPos = _camera.transform.position;
        int take = Mathf.Min(_maxView, _visibleCandidates.Count);
        SelectTop(_visibleCandidates, take, camPos);

        _selectedView.Clear();
        for (int i = 0; i < take; i++) _selectedView.Add(_visibleCandidates[i]);
        // 숨길 대상을 큐에 넣기 
        for (int i = 0; i < _activeView.Count; i++)
        {
            int idx = _activeView[i];
            if (_selectedView.Contains(idx)) continue;
            if (_cullingGroup.GetDistance(idx) <= 1) continue; // 가까우면 유지!
            _pendingSpawn.Remove(idx); // spawn 예약이 있던 애가 hide 대상이 되면 spawn 예약 취소
            if (_pendingHide.Add(idx))  _hideQueue.Enqueue(idx); // 중복 방지
        }

        // 생성할 대상 Queue에 넣기! (즉, 바로 즉시 spawn 하지 말고 예약!)
        for (int i = 0; i < take; i++)
        {
            int idx = _visibleCandidates[i];
            var data = _monsters[idx];
            if (data.hasView && data.viewObj != null) continue;
            if (_monsters[idx].hp <= 0f) continue; // 죽은 애도 안되게
            _pendingHide.Remove(idx); // hide 예약이 되어있던 애가 spawn 대상이 되면 hide 예약 취소!
            if (_pendingSpawn.Add(idx)) _spawnQueue.Enqueue(idx);
        }
    }

    private void ProcessQueue()
    {
        int hideBudget = _hideBudgetPerFrame;
        while (hideBudget-- > 0 && _hideQueue.Count > 0)
        {
            int idx = _hideQueue.Dequeue();
            if (!_pendingHide.Remove(idx)) continue;

            var data = _monsters[idx];
            if (!data.hasView || data.viewObj == null) continue;

            var control = data.viewObj.GetComponent<MonsterController>();
            if (control == null || !control.gameObject.activeInHierarchy || control.IsDead)
            {
                _activeMonster.Remove(control);
                _poolManager.Release(data.viewObj);
                data.viewObj = null;
                data.hasView = false;
                _monsters[idx] = data;
                continue;
            }

            control.SleepMonster(ref data);
            _poolManager.Release(data.viewObj);

            _activeMonster.Remove(control);
            data.viewObj = null;
            data.hasView = false;
            _monsters[idx] = data;
        }

        // spawn 처리
        int spawnBudget = _spawnBudgetPerFrame;
        while (spawnBudget-- > 0 && _spawnQueue.Count > 0)
        {
            int idx = _spawnQueue.Dequeue();
            if (!_pendingSpawn.Remove(idx)) continue;

            var data = _monsters[idx];
            if (data.hasView && data.viewObj != null) continue;

            var obj = _poolManager.Spawn(data.poolKey);
            if (obj == null) break; // 풀부족

            var control = obj.GetComponent<MonsterController>();
            control.CreateMonster(in data, _player);

            _activeMonster.Add(control);
            data.viewObj = obj;
            data.hasView = true;
            _monsters[idx] = data;
        }
    }

    private void SelectTop(List<int> list, int k, Vector3 camPos)
    {

        // O(N * K)
        for (int i = 0; i < k; i++)
        {
            int best = i;
            float bestDist = (_monsters[list[i]].pos - camPos).sqrMagnitude;

            for (int j = i + 1; j < list.Count; j++)
            {
                float d = (_monsters[list[j]].pos - camPos).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = j;
                }
            }

            if(best != i)
            {
                int temp = list[i];
                list[i] = list[best];
                list[best] = temp;
            }
        }
    }

    private void HideMonsterTick(Vector3 playerPos)
    {
        float now = Time.time;

        for(int i = 0; i < _monsters.Count; i++)
        {
            if (_monsters[i].hasView) continue;

            var data = _monsters[i];
            if (now < data.stunRemain) continue;
            if (now < data.nextNavTickTime) continue;
           
            Vector3 moveToPlayer = playerPos - data.pos;
            float dist = moveToPlayer.magnitude;
            if (dist < 0.1f) continue;

            Vector3 dir = moveToPlayer / dist;

            float step = data.navMoveSpeed * _navTick;
            data.prevPos = data.pos; // 이동 직전 저장
            data.pos += dir * Mathf.Min(step, dist);
            data.rot = Quaternion.LookRotation(dir);
            data.nextNavTickTime = now + _navTick;
            
            AvoidBlock(ref data); // 안보이는데 이동할 때 벽에 끼거나 하는거 방지하기 위해

            _monsters[i] = data;

        }
    }


    private void AvoidBlock(ref MonsterData monsterData)
    {
        if (Random.value > 0.25f) return;
        if (NavMesh.SamplePosition(monsterData.pos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas)) monsterData.pos = hit.position;
    }

    public void RemoveActive(MonsterController monsterController) => _activeMonster.Remove(monsterController);


    public void ApplyStunInRadius(Vector3 center, float radius, float stunTime)
    {
        float r2 = radius * radius;
        float end = Time.time + stunTime;

        for (int i = 0; i < _monsters.Count; i++)
        {
            var data = _monsters[i];
            if (data.hp <= 0f) continue;

            Vector3 pos = data.pos;
            MonsterController ctrl = null;

            if (data.viewObj != null) 
            {
                ctrl = data.viewObj.GetComponent<MonsterController>();
                if (ctrl != null) pos = ctrl.transform.position; 
            }

            if ((pos - center).sqrMagnitude > r2) continue;
            if (end > data.stunRemain) data.stunRemain = end;

            if (ctrl != null) ctrl.ApplyStun(stunTime);

            data.pos = pos;

            _monsters[i] = data;
        }
    }
    public void ApplyDamageInRadius(Vector3 center, float radius, float damage)
    {
        float r2 = radius * radius;

        for (int i = 0; i < _monsters.Count; i++)
        {
            var data = _monsters[i];
            if (data.hp <= 0f) continue;

            Vector3 pos = data.pos;
            MonsterController ctrl = null;
            if (data.viewObj != null)
            {
                ctrl = data.viewObj.GetComponent<MonsterController>();
                if (ctrl != null) pos = ctrl.transform.position;
            }

            if ((pos - center).sqrMagnitude > r2) continue;

            if (ctrl != null) ctrl.TakeSingleDamage(damage);
            else
            {
                float before = data.hp;
                data.hp -= damage;

                if (data.hp <= 0f)
                {
                    data.hp = 0f;
                    data.stunRemain = 0f;
                    if (before > 0f) AliveCount--;

                    // 숨김 몬스터는 컨트롤러가 없으니 여기서 드랍
                    _dropManager.Drop(pos, 1);
                }
                _monsters[i] = data;
            }
        }
    }
    public void ApplyDamageInRadiusSplash(Vector3 center, float radius, float damage, MonsterController target, string weaponId = null)
    {
        float r2 = radius * radius;
        for (int i = 0; i < _monsters.Count; i++)
        {
            var data = _monsters[i];
            if (data.hp <= 0f) continue;
            Vector3 pos = data.pos;
            MonsterController ctrl = null;
            if (data.viewObj != null)
            {
                ctrl = data.viewObj.GetComponent<MonsterController>();
                if (ctrl != null)
                {
                    if (target != null && ctrl == target) continue; //타겟 제외
                    pos = ctrl.transform.position;
                }
            }
            if ((pos - center).sqrMagnitude > r2) continue;
            if (ctrl != null)
            {
                ctrl.TakeSingleDamage(damage, weaponId);
            }
            else
            {
                float before = data.hp;
                data.hp -= damage;
                bool killed = data.hp <= 0f;
                GameManager.Instance.GameStats.AddDamage(weaponId, damage, killed); 

                if (killed)
                {
                    data.hp = 0f;
                    if (before > 0f) AliveCount--;
                    _dropManager.Drop(pos, 1);
                }

                _monsters[i] = data;
            }
        }
    }

    public MonsterController FindClosestVisibleMonster(Vector3 origin, float radius)
    {
        float r2 = radius * radius;
        float best = float.MaxValue;
        MonsterController bestCtrl = null;

        for (int i = 0; i < _monsters.Count; i++)
        {
            var data = _monsters[i];
            if (data.hp <= 0f) continue;
            if (data.viewObj == null) continue; 
            var ctrl = data.viewObj.GetComponent<MonsterController>();
            if (ctrl == null || ctrl.IsDead) continue;
            float d2 = (ctrl.transform.position - origin).sqrMagnitude;
            if (d2 > r2) continue;
            if (d2 < best)
            {
                best = d2;
                bestCtrl = ctrl;
            }
        }
        return bestCtrl;
    }

    public MonsterController FindClosestVisibleMonsterExclude(Vector3 origin, float radius, MonsterController exclude){
        float r2 = radius * radius;
        float best = float.MaxValue;
        MonsterController bestCtrl = null;
        for (int i = 0; i < _monsters.Count; i++)
        {
            var data = _monsters[i];
            if (data.hp <= 0f) continue;
            if (data.viewObj == null) continue;
            var ctrl = data.viewObj.GetComponent<MonsterController>();
            if (ctrl == null || ctrl.IsDead) continue;
            if (ctrl == exclude) continue; // 자기자신이면 NONO

            float d2 = (ctrl.transform.position - origin).sqrMagnitude;
            if (d2 > r2) continue;
            if (d2 < best) { best = d2; bestCtrl = ctrl; }
        }
        return bestCtrl;
    }

    public void ApplyLineDamage(Vector3 p1, Vector3 p2, float hitRadius, float damage, string weaponId = null)
    {
        float r = hitRadius;

        for (int i = 0; i < _monsters.Count; i++)
        {
            var data = _monsters[i];
            if (data.hp <= 0f) continue;

            Vector3 pos = data.pos;
            MonsterController ctrl = null;
            if (data.viewObj != null)
            {
                ctrl = data.viewObj.GetComponent<MonsterController>();
                if (ctrl != null) pos = ctrl.transform.position;
            }
            if (!Util.IsLineSegmentIntersectingCircleXZ(p1, p2, pos, r, out _)) continue;
            if (ctrl != null)
            {
                ctrl.TakeSingleDamage(damage, weaponId);
            }
            else
            {
                float before = data.hp;
                data.hp -= damage;

                bool killed = data.hp <= 0f;
                GameManager.Instance.GameStats.AddDamage(weaponId, damage, killed); 
                if (killed)
                {
                    data.hp = 0f;
                    data.stunRemain = 0f;
                    if (before > 0f) AliveCount--;
                    _dropManager.Drop(pos, 1);
                }
                _monsters[i] = data;
            }
        }
    }

}