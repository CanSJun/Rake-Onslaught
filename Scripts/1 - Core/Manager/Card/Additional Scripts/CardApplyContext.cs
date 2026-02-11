using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardApplyContext
{
    public GameManager Game { get; }
    public PlayerController Player { get; }
    public MoveSystem Move { get; }
    public PlayerCombatSystem Combat { get; }
    public SkillManager SkillManager { get; }

    public ExpDropManager ExpDropManager => Game != null ? Game.ExpDropManager : null;
    public PlayerExpManager PlayerExpManager => Game != null ? Game.PlayerExpManager : null;

    private readonly IWeaponCardEffectReceiver[] _weapons;
    private readonly int _weaponCount;

    public CardApplyContext( GameManager game, PlayerController player, MoveSystem move, PlayerCombatSystem combat, SkillManager skillManager, IWeaponCardEffectReceiver[] weapons, int weaponCount)
    {
        Game = game;
        Player = player;
        Move = move;
        Combat = combat;
        SkillManager = skillManager;
        _weapons = weapons ?? Array.Empty<IWeaponCardEffectReceiver>();
        _weaponCount = Mathf.Clamp(weaponCount, 0, _weapons.Length);
    }

    public static CardApplyContext CreateCardContext()
    {
        var gm = GameManager.Instance;
        var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        MoveSystem move = null;
        PlayerCombatSystem combat = null;
        SkillManager skillManager = null;
        IWeaponCardEffectReceiver[] weapons = Array.Empty<IWeaponCardEffectReceiver>();
        int weaponCount = 0;
        if (player != null)
        {
            move = player.Move;
            combat = player.Combat;
         
            if (combat != null && combat.SkillSystem != null) skillManager = combat.SkillSystem.Manager;

            var monos = player.GetComponentsInChildren<MonoBehaviour>(true);
            if (monos != null && monos.Length > 0)
            {
                weapons = new IWeaponCardEffectReceiver[monos.Length];
                for (int i = 0; i < monos.Length; i++)
                {
                    var mb = monos[i];
                    if (mb is IWeaponCardEffectReceiver w) weapons[weaponCount++] = w;
                }
            }
        }
        return new CardApplyContext(gm, player, move, combat, skillManager, weapons, weaponCount);
    }

    public void ApplyEffectToWeapons(string targetId, in CardEffect effect)
    {
        if (_weaponCount <= 0) return;
        if (string.IsNullOrEmpty(targetId))
        {
            for (int i = 0; i < _weaponCount; i++)
            {
                var w = _weapons[i];
                if (w != null) w.TryApplyCardEffect(effect);
            }
            return;
        }

        for (int i = 0; i < _weaponCount; i++)
        {
            var w = _weapons[i];
            if (w == null) continue;
            if (string.Equals(w.WeaponId, targetId, StringComparison.Ordinal)) w.TryApplyCardEffect(effect); // 추후에 언어 차이 추가를 위해 ordinal
        }
    }
    public void UnlockExistingWeapons(CardRuntimeManager runtime)
    {
        if (runtime == null) return;
        for (int i = 0; i < _weaponCount; i++)
        {
            var w = _weapons[i];
            if (w == null) continue;
            runtime.UnlockWeapon(w.WeaponId);
        }
    }
}
