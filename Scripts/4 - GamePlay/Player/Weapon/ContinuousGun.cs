using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContinuousGun : GunBase
{
    protected bool _isFiring = false;

    public override void Attack()
    {
        if (!_isFiring) return;
        if (IsReloading) return;
        if (Time.time - _lastFireTime < _fireRate) return;
        if (!TryConsumeAmmo(GetAmmoCost())) return;
        _lastFireTime = Time.time;
        Fire();
    }

    public virtual void StartFire() => _isFiring = true;
    public virtual void StopFire() => _isFiring = false;

    public override void OnDown() => StartFire(); 
    public override void OnHold() => Attack();
    public override void OnUp() => StopFire();



}
