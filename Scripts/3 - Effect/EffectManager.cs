using UnityEngine;

public class EffectManager : MonoBehaviour
{

    private ParticleSystem _particleSystem;
    private ParticleSystemRenderer _particleSystemRenderer;
    private ParticleSystemRenderer[] _particleSystemRendererChilds;
    private bool _isPlaying = false;

    private MaterialPropertyBlock _materialPropertyBlock;
    private static readonly int BaseColor = Shader.PropertyToID("_UnlitColor");




    protected virtual void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        _particleSystemRendererChilds = GetComponentsInChildren<ParticleSystemRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
    }


    protected virtual void OnEnable()
    {
        _isPlaying = true;
        if(_particleSystem != null)_particleSystem.Play();
    }

    protected virtual void Update()
    {
        if (_isPlaying && !_particleSystem.IsAlive())
        {
            _isPlaying = false;
            GameManager.Instance.PoolManager.Release(gameObject);
        }
    }
    protected virtual void OnDisable()
    {
        _isPlaying = false;
    }

    public void ChangeEffectColor(Color color)
    {
        color.a = 1f;


        _materialPropertyBlock.Clear();
        _materialPropertyBlock.SetColor(BaseColor, color);

        _particleSystemRenderer.SetPropertyBlock(_materialPropertyBlock);

        for (int i = 0; i < _particleSystemRendererChilds.Length; i++)
        {
            _particleSystemRendererChilds[i].SetPropertyBlock(_materialPropertyBlock);
        }

    }
}