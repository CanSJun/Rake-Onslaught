using UnityEngine;

public class ImpactColorManager : MonoBehaviour
{

    private ParticleSystemRenderer _particleSystemRenderer;
    private ParticleSystemRenderer[] _particleSystemRendererChilds;

    private MaterialPropertyBlock _materialPropertyBlock;
    private static readonly int BaseColor = Shader.PropertyToID("_UnlitColor");
    protected void Awake()
    {
        _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        _particleSystemRendererChilds = GetComponentsInChildren<ParticleSystemRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
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