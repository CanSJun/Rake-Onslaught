using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotManager 
{
    [SerializeField] TextMeshProUGUI _ammoText;
    [SerializeField] Image _weaponImage;
    [SerializeField] List<Image> _slots = new();
    [SerializeField] Color _unUsedColor;

    private List<Image> _lockIcons = new();
    private bool[] _locked;
    private int _slotIndex;

    private int _lastCurrent = int.MinValue;
    private int _lastMax = int.MinValue;
    private bool _lastReloading;


    public bool IsLocked(int index)
    {
        if (_locked == null || index < 0 || index >= _locked.Length) return false;
        return _locked[index];
    }

    public void SetLocked(int index, bool locked)
    {
        if (_locked == null || index < 0 || index >= _locked.Length) return;
        _locked[index] = locked;
        if (_lockIcons != null && index < _lockIcons.Count && _lockIcons[index] != null) _lockIcons[index].gameObject.SetActive(locked);
        UpdateSelect();
    }

    public void Initialize(TextMeshProUGUI ammoText, Image weaponImage, List<Image> slots, Color unUsedColor, List<Image> lockIcons)
    {
        _ammoText = ammoText;
        _weaponImage = weaponImage;
        _slots = slots;
        _unUsedColor = unUsedColor;
        _lockIcons = lockIcons;
        _locked = new bool[_slots.Count];
        for (int i = 1; i < _locked.Length; i++) _locked[i] = true;
    }
    


    public void ChangeSlotIndex(int slotIndex, int currentAmmo, int maxAmmo, bool isReloading)
    {
        _slotIndex = slotIndex;
        UpdateSelect();
        UpdateWeaponImage();
        UpdateAmmo(currentAmmo, maxAmmo, isReloading);
    }
    public int SlotIndex => _slotIndex;
    public void UpdateSelect() {
        if (_slots == null) return;

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot == null) continue;
            if (IsLocked(i)) continue;
            slot.color = (i == _slotIndex) ? Color.white : _unUsedColor;
        }
    }
    public void UpdateWeaponImage()
    {
        if (_weaponImage == null || _slots == null || _slotIndex < 0 || _slotIndex >= _slots.Count) return;
        _weaponImage.sprite = _slots[_slotIndex].sprite;
    }
    public void UpdateAmmo(int current, int max, bool isReloading)
    {
        if (_ammoText == null) return;

        if (_lastCurrent == current && _lastMax == max && _lastReloading == isReloading) return;
        _lastCurrent = current;
        _lastMax = max;
        _lastReloading = isReloading;

        if (max == int.MaxValue)
        {
            _ammoText.SetText("¡Ä");
            return;
        }

        if (isReloading)
        {
            _ammoText.SetText("RELOAD");
            return;
        }

        _ammoText.SetText("{0}/{1}", current, max);
    }

}
