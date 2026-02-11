using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EMPscript : MonoBehaviour
{
    [SerializeField] private Transform _transform;
    [SerializeField] private LayerMask _monsterLayer;
    [SerializeField] private float _shakeDuration;
    [SerializeField] private float _shakePower;
    private EMPSkillData _data;
    private int _level;
    private float _time;
    private bool _applied;
    private static readonly Collider[] _hits = new Collider[32];

    private SoundManager _sound;

    private void Awake()
    {
        _sound = SoundManager.Instance;
    }
    public void Initialize(EMPSkillData data, int level)
    {
        _data = data;
        _level = level;
        _time = 0f;
        _applied = false;
        _transform.localScale = Vector3.zero;
        
        Util.ShakeCamera(_shakeDuration, _shakePower);
        _sound.PlaySfx(SfxId.EMP);
    }
    private void Update()
    {
        Expand();
    }

    private void Expand()
    {
        _time += Time.deltaTime;
        float t = Mathf.Clamp01(_time / _data.expandDuration);
        float radius = _data.GetRadius(_level);
        _transform.localScale = Vector3.one * radius * 2f * t;

        if (t >= 1f)
        {
            CheckApply(); 
            Finish();    
        }
    }
    private void CheckApply()
    {
        if (_applied) return;
        _applied = true;
        float radius = _data.GetRadius(_level);
        float stunTime = _data.GetStunDuration(_level);

        GameManager.Instance.MonsterViewManager.ApplyStunInRadius(transform.position, radius, stunTime);

    }

    private void Finish() => GameManager.Instance.PoolManager.Release(gameObject);
    

}
