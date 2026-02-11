using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShoot
{
    void SetWeapon(GunBase weapon);
    void Tick();
}
