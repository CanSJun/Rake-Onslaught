using UnityEngine;

public  class MissileScript : MonoBehaviour 
{
    [SerializeField] private float _shakeDuration;
    [SerializeField] private float _shakePower;
    private Vector3 _p0, _p1, _p2, _p3;
    private float _t;
    private float _damage;


    private MonsterController _targetCtrl;
    private Vector3 _fallbackEnd;
    private bool _targetLostCheck;

    private GameManager _gameManager;
    private float _travelTime = 0.6f;
    private float _explodeRadius;
    private float _splashRadiusDamage;

    private Transform _transform;

    private SoundManager _sound;
    private string _skillId;
    private bool check = false;
    private void Awake()
    {
        _transform = transform;
        _gameManager = GameManager.Instance;
        _sound = SoundManager.Instance;
    }
    public void Initialize(MissileSkillData skill, Vector3 direction)
    {
        if (skill == null) return;

        
        _targetLostCheck = false;
        _damage = skill.damage;
        _splashRadiusDamage = skill.splashDamageMul;
        _explodeRadius = skill.splashRadius;
        _p0 = transform.position;
        _skillId = skill.skillID;
        _targetCtrl = _gameManager.MonsterViewManager.FindClosestVisibleMonster(_p0, skill.range);

        _fallbackEnd = _p0 + direction * skill.range;

        Vector3 endPos = (_targetCtrl != null) ? _targetCtrl.transform.position : _fallbackEnd;
        _p3 = endPos;

        Vector3 randomVector = Random.insideUnitSphere * skill.randomOffset;

        _p1 = _p0 + Vector3.up * 2f + randomVector;
        _p2 = _p3 + Vector3.up * 2f + randomVector;

        _t = 0f;

        _travelTime = Mathf.Max(0.05f, _travelTime);
        _sound.PlaySfx(SfxId.MissileLaunch);
        check = true;
    }
    private void OnDrawGizmos()
{
    Gizmos.DrawSphere(transform.position, 0.2f);
}
    private void Update()
    {
        if (check == false) return;
        _t += Time.deltaTime / _travelTime;
        _t = Mathf.Min(_t, 1f);

        if (!_targetLostCheck && IsTargetValid(_targetCtrl))
        {
            _p3 = _targetCtrl.transform.position;
            _p2 = _p3 + Vector3.up * 2f;
        }
        else
        {
            _targetLostCheck = true;
        }
        Move();

        if (_t >= 1f) OnArrive();

    }

    private void Move()
    {
        Vector3 next = SkillUtil.BezierCurve(_p0, _p1, _p2, _p3, _t);
        _transform.position = next;
    }

    private bool IsTargetValid(MonsterController ctrl)
    {
        if (ctrl == null) return false;
        if (ctrl.IsDead) return false;
        if (!ctrl.IsViewActive) return false; 
        return true;
    }

    private void OnArrive()
    {
        Util.ShakeCamera(_shakeDuration, _shakePower);
        var effect = _gameManager.PoolManager.Spawn(PoolKeys.MissileEffect);
        effect.transform.position = transform.position;
        effect.transform.rotation = Quaternion.identity;


        if (IsTargetValid(_targetCtrl)) _targetCtrl.TakeSingleDamage(_damage);

        _sound.PlaySfxAt(SfxId.MissileImpact, transform.position, maxDist: 20f);
        // 주변 스플래시(타겟 제외)
        float splashDamage = _damage * _splashRadiusDamage;
        _gameManager.MonsterViewManager.ApplyDamageInRadiusSplash(transform.position, _explodeRadius, splashDamage, _targetCtrl, _skillId);
        _gameManager.PoolManager.Release(gameObject);
    }

}
