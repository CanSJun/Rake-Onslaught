using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct SkillSlotView
{
    public SkillSlot slot;
    public Image icon;
    public Image cooldownMask;
    public Text cooldownText;

    public Image lockIcon;
}

public class SkillSlotManager
{
    private readonly Dictionary<SkillSlot, SkillSlotView> _views = new();
    private readonly Dictionary<SkillSlot, Color> _maskBaseColor = new();
    private readonly Dictionary<SkillSlot, int> _lastShownSeconds = new();

    private readonly HashSet<SkillSlot> _lockedSlots = new();

    public void Initialize(IEnumerable<SkillSlotView> views)
    {
        _views.Clear();
        _maskBaseColor.Clear();
        _lastShownSeconds.Clear();

        foreach (var v in views)
        {
            if (v.icon == null) continue;
            _views[v.slot] = v;
            if (v.cooldownMask != null)
            {
                _maskBaseColor[v.slot] = v.cooldownMask.color;
                v.cooldownMask.type = Image.Type.Filled;
                v.cooldownMask.fillAmount = 0f;
                v.cooldownMask.enabled = false;
            }

            if (v.cooldownText != null)
            {
                v.cooldownText.text = "";
                _lastShownSeconds[v.slot] = -1;
            }

            if (v.lockIcon != null) v.lockIcon.enabled = false;
        }
    }
    public bool IsLocked(SkillSlot slot) => _lockedSlots.Contains(slot);

    public void SetLocked(SkillSlot slot, bool locked)
    {
        if (!_views.TryGetValue(slot, out var v)) return;
        if (locked) _lockedSlots.Add(slot);
        else _lockedSlots.Remove(slot);
        if (v.icon != null)
        {
            if (locked) v.lockIcon.gameObject.SetActive(true);
            else v.lockIcon.gameObject.SetActive(false);
        }

        if (v.lockIcon != null) v.lockIcon.enabled = locked;

        if (locked)
        {
            if (v.cooldownMask != null)
            {
                v.cooldownMask.enabled = false;
                v.cooldownMask.fillAmount = 0f;
            }

            if (v.cooldownText != null)
            {
                v.cooldownText.text = "";
                _lastShownSeconds[slot] = 0;
            }
        }
    }
    public void SetCooldown(SkillSlot slot, float remaining, float duration)
    {
        if (!_views.TryGetValue(slot, out var v)) return;

        if (IsLocked(slot))
        {
            if (v.cooldownMask != null)
            {
                v.cooldownMask.enabled = false;
                v.cooldownMask.fillAmount = 0f;
            }
            if (v.cooldownText != null) v.cooldownText.text = "";
            return;
        }

        bool isCooling = remaining > 0f && duration > 0f; // 남아 있는 쿨타임이 0보다 커도 쿨타임! 
        if (v.cooldownMask != null)
        {
            v.cooldownMask.enabled = isCooling;
            if (isCooling)
            {
                float t = Mathf.Clamp01(remaining / duration); // 1 -> 0
                v.cooldownMask.fillAmount = t;
                
                // 알파도 줄여서 어두운 곳에서 점점 밝아지게!
                if (_maskBaseColor.TryGetValue(slot, out var baseColor))
                {
                    var c = baseColor;
                    c.a = baseColor.a * t;
                    v.cooldownMask.color = c;
                }
            }
            else
            {
                v.cooldownMask.fillAmount = 0f;
                if (_maskBaseColor.TryGetValue(slot, out var baseColor)) v.cooldownMask.color = baseColor;
            }
        }

        if (v.cooldownText != null)
        {
            if (isCooling)
            {
                int sec = Mathf.CeilToInt(remaining);
                if (!_lastShownSeconds.TryGetValue(slot, out var last) || last != sec)
                {
                    v.cooldownText.text = sec.ToString();
                    _lastShownSeconds[slot] = sec;
                }
            }
            else
            {
                if (!_lastShownSeconds.TryGetValue(slot, out var last) || last != 0)
                {
                    v.cooldownText.text = "";
                    _lastShownSeconds[slot] = 0;
                }
            }
        }
    }
}
