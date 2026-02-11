using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public abstract class DecalEffectManager : MonoBehaviour
{
    protected abstract string PoolKey { get; }

    private float _timer;
    [SerializeField] private float _lifeTime = 5f;
    [SerializeField] private float _fadeTime = 2f;

    private DecalProjector _decal;

    protected virtual void Awake()
    {
        _decal = GetComponentInChildren<DecalProjector>();
    }

    protected virtual void OnEnable()
    {
        _timer = 0f;
        SetOpacity(1f);
    }

    protected virtual void Update()
    {
        _timer += Time.deltaTime;

        if (_timer > _lifeTime)
        {
            float t = (_timer - _lifeTime) / _fadeTime;
            SetOpacity(1f - t);

            if (t >= 1f) GameManager.Instance.PoolManager.Release(gameObject);
        }
    }

    protected void SetOpacity(float value)
    {
        _decal.fadeFactor = Mathf.Clamp01(value);
    }
}
