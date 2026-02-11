using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPScript : MonoBehaviour
{
    [SerializeField] private float _hp = 10f;
    [GetComponents("HP Gage")] private HPBarGraphic _hPBarGraphic;
    
    [GetComponents("HP Text")] private TextMeshProUGUI _hpText;

    private int _lastHpInt = int.MinValue;

    private bool _deadCalled;
    private void Start()
    {
        Util.ApplyComponents(this);
        _hpText.SetText("{0}", _hp);
    }

    public void SetHp(float value)
    {
        float currentHp = _hPBarGraphic._value * _hp;
        currentHp = Mathf.Max(0f, currentHp - value);
        _hPBarGraphic._value = currentHp / _hp;
        _hPBarGraphic.SetFill();
        UpdateHpText(currentHp);

        if (!_deadCalled && currentHp <= 0f)
        {
            _deadCalled = true;
            GameManager.Instance?.OnPlayerDead();
            return;
        }
        Util.ShakeCamera(0.1f, 0.2f);
    }
    private void UpdateHpText(float currentHp)
    {
        int hpInt = Mathf.CeilToInt(currentHp);
        if (hpInt == _lastHpInt) return; 
        _lastHpInt = hpInt;
        _hpText.SetText("{0}", hpInt); 
    }
}
