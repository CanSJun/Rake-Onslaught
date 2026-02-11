using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    private TrailRenderer _trailRenderer;
    private MaterialPropertyBlock _materialPropertyBlock;
    private static readonly int BaseColor = Shader.PropertyToID("_UnlitColor");
    private Color _color;

    private Transform _transform;
    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
        _transform = transform;
    }

    public void SetColor(Color color)
    {
        _color = color;
        _color.a = 1f;
        _materialPropertyBlock.SetColor(BaseColor, _color);
        _trailRenderer.SetPropertyBlock(_materialPropertyBlock);

    }

    private void OnEnable()
    {
       _trailRenderer.enabled = false;
        _trailRenderer.Clear();
        _trailRenderer.enabled = true;
       _trailRenderer.SetPropertyBlock(null);
    }

    public void SetPosition(Vector3 pos) => _transform.position = pos;
    public Color Color => _color;
}
