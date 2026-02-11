using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextScript : MonoBehaviour
{

    private TextMeshProUGUI _textMeshPro;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Sequence _sequence;

    [SerializeField] private Gradient _normalGradient;
    [SerializeField] private Gradient _criticalGradient;

    private VertexGradient _normalVertexGradient;
    private VertexGradient _criticalVertexGradient;

    [SerializeField] private float MOVE_Y = 60f;
    [SerializeField] private float MOVE_TIME = 0.75f;
    [SerializeField] private float SCALE_UP = 1.25f;
    [SerializeField] private float SCALE_TIME = 0.15f;
    [SerializeField] private float FADE_TIME = 0.8f;

    private PoolManager _poolManager;
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _textMeshPro = GetComponent<TextMeshProUGUI>();

        _normalVertexGradient = new VertexGradient(_normalGradient.Evaluate(0), _normalGradient.Evaluate(0), _normalGradient.Evaluate(1), _normalGradient.Evaluate(1));
        _criticalVertexGradient = new VertexGradient(_criticalGradient.Evaluate(0), _criticalGradient.Evaluate(0), _criticalGradient.Evaluate(1), _criticalGradient.Evaluate(1));

    }

    private void Start()
    {
        _poolManager = GameManager.Instance.PoolManager;
    }
    public void OnPlay(float damage)
    {
        if (_sequence != null) _sequence.Kill(false);

        _textMeshPro.SetText("{0}", (int)damage);

        int idx = Random.Range(0, 2);
        _textMeshPro.colorGradient = (idx == 0) ? _normalVertexGradient : _criticalVertexGradient;

        _canvasGroup.alpha = 1f;
        _rectTransform.localScale = Vector3.one;

        Vector2 startPos = _rectTransform.anchoredPosition;

        _sequence = DOTween.Sequence();
        _sequence.Append(_rectTransform.DOScale(SCALE_UP, SCALE_TIME).SetEase(Ease.OutBack))
                 .Join(_rectTransform.DOAnchorPos(startPos + Vector2.up * MOVE_Y, MOVE_TIME).SetEase(Ease.OutQuad))
                 .Join(_canvasGroup.DOFade(0f, FADE_TIME))
                 .OnComplete(() =>
                 {
                     _poolManager.Release(gameObject);
                 });
    }
}
