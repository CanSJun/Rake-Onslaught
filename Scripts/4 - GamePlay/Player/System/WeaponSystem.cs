using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class WeaponSystem
{
    private IInput _input;
    private GunBase[] _guns;
    private int _currentIndex;


    private GunBase _weapon;
    public GunBase CurrentGun => _guns[_currentIndex];
    public void Initialize(IInput input, GunBase[] guns)
    {
        _input = input;
        _guns = guns;
        _currentIndex = 0;
        SetWeapon(_currentIndex);
    }

    public void SetWeapon(int index)
    {
        if (index < 0 || index >= _guns.Length) return;
        _currentIndex = index;
        _weapon = _guns[_currentIndex];
    }

    public void Tick()
    {
        if (_input.ReloadDown) _weapon.RequestReload();
        if (!_input.IsAim) return;
        if (_input.LeftButtonDown) _weapon.OnDown();
        if (_input.IsShoot) _weapon.OnHold();
        if (_input.LeftButtonUp) _weapon.OnUp();
    }
}