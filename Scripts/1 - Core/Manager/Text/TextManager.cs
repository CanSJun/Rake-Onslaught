using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;


public class TextManager
{
    private  RectTransform _rectTransform;

    private Camera _camera;


    public void Initialize(GameObject damageTextPrefab, RectTransform rectTransform)
    {
        _rectTransform = rectTransform;
        _camera = Camera.main;
      //  GameManager.Instance.PoolManager.CreatePool(PoolKeys.DamageTextpoolName, damageTextPrefab, rectTransform);
    }


    public void DamageTextSpawn( float damage, Vector3 worldPos)
    {
        if (_rectTransform == null || _camera == null) return;
        Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);
        GameObject obj = GameManager.Instance.PoolManager.Spawn(PoolKeys.DamageText);
        var rt = (RectTransform)obj.transform;
        rt.SetParent(_rectTransform, false);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenPos, null, out var localPos);
        rt.anchoredPosition = localPos;
        if (obj.TryGetComponent<DamageTextScript>(out var txt)) txt.OnPlay(damage);
    }
}
