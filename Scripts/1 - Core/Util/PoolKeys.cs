
using System;
using System.Collections.Generic;
using UnityEngine;

public static class PoolKeys
{
    private static readonly Dictionary<string, string> _keys = new();

    public static void Register(string id)
    {
        if (!_keys.ContainsKey(id))
            _keys.Add(id, id);

        Debug.Log($"{id} 등록 완료");
    }

    public static string Get(string id)
    {
        if (_keys.TryGetValue(id, out var key)) return key;
        Debug.LogWarning($"Pool key not found: {id}");
        return null;
    }

    public static string MonsterBlood => Get("MonsterBlood");
    public static string MonsterGroundBlood => Get("MonsterGroundBlood");
    public static string Bullet => Get("Bullet");
    public static string ImpactEffect => Get("ImpactEffect");
    public static string DamageText => Get("DamageText");
    public static string Missile => Get("Missile");
    
    public static string MissileEffect => Get("MissileEffect");
    public static string Rake => Get("Rake");

    public static string EMP => Get("EMP");

    public static string ExpBall => Get("ExpBall");

}