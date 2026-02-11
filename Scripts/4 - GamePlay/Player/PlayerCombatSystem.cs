using System.Collections.Generic;
using UnityEngine;

public  class PlayerCombatSystem
{
    private WeaponSystem _weaponSystem;
    private SkillSystem _skillSystem;

    public SkillSystem SkillSystem => _skillSystem;
    public void Initialize( IInput input, Transform owner, GunBase[] guns, Dictionary<SkillSlot, SkillScriptableObject> skillData)
    {
        _weaponSystem = new WeaponSystem();
        _skillSystem = new SkillSystem();
        _weaponSystem.Initialize(input, guns);
        _skillSystem.Initialize(input, owner, skillData);
    }
    public void Tick()
    {
        _weaponSystem.Tick();
        _skillSystem.Tick();
    }

    public void ChangeWeapon(int index) => _weaponSystem.SetWeapon(index);
    public GunBase CurrentGun => _weaponSystem.CurrentGun;
}
