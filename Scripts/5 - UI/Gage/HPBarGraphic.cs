
using UnityEngine;
using UnityEngine.UI;

public class HPBarGraphic : Graphic
{
    [Header("Gage Setting")]
    public int _count = 10;
    public float _width = 19.5f;
    public float _height = 38f;
    public float _spacing = 2f;

    [Header("Value")]
    [Range(0f, 1f)] public float _value = 1f;

    [Header("Color Gradient")]
    public Gradient _gradient;


    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        int active = Mathf.RoundToInt(_count * _value);
        float x = 0f;
        Color c = _gradient.Evaluate(_value);
        for (int i = 0; i < _count; i++)
        {
            if (i < active) AddBlock(vertexHelper, x, c);
            
            x += _width + _spacing;
        }
    }

    private void AddBlock(VertexHelper vertexHelper, float xStart, Color color)
    {
        int idx = vertexHelper.currentVertCount;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = new Vector2(xStart, 0);
        vertexHelper.AddVert(vertex);

        vertex.position = new Vector2(xStart + _width, 0);
        vertexHelper.AddVert(vertex);

        vertex.position = new Vector2(xStart + _width, _height);
        vertexHelper.AddVert(vertex);

        vertex.position = new Vector2(xStart, _height);
        vertexHelper.AddVert(vertex);

        vertexHelper.AddTriangle(idx, idx + 1, idx + 2);
        vertexHelper.AddTriangle(idx, idx + 2, idx + 3);
    }

    public void SetFill()
    {
        _value = Mathf.Clamp01(_value);
        SetVerticesDirty();
    }

}
