using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpGage : MonoBehaviour, IGameInitializable
{
    private Image _gageImage;
    private PlayerExpManager _exp;

    private void Awake()
    {
        _gageImage = GetComponent<Image>();
    }

    private void Start()
    {
        _exp = GameManager.Instance.PlayerExpManager;
    }
    public void OnGameInitialized()
    {
        Bind(_exp);
    }

    public void Bind(PlayerExpManager exp)
    {
        if (_exp != null)
        {
            _exp.OnChanged -= OnExpChanged;
        }

        _exp = exp;

        if (_exp != null)
        {
            _exp.OnChanged += OnExpChanged;
            OnExpChanged(_exp.CurrentExp, _exp.ExpToNext, _exp.Level);
        }
    }


    private void OnDisable()
    {
        if (_exp != null)
        {
            _exp.OnChanged -= OnExpChanged;
        }
    }
    private void OnExpChanged(int cur, int next, int level)
    {
        if (_gageImage == null) return;
        _gageImage.fillAmount = next <= 0 ? 0f : (float)cur / next;
    }


}