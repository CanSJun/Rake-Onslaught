using System.Collections.Generic;

public sealed class GameStats
{
    public struct WeaponStat
    {
        public float damage;
        public int kills;
    }

    private readonly Dictionary<string, WeaponStat> _weaponList = new();
    public IReadOnlyDictionary<string, WeaponStat> Weapon => _weaponList;
    public int TotalExp { get; private set; }
    public int TotalKills { get; private set; }
    public void Reset()
    {
        _weaponList.Clear();
        TotalExp = 0;
        TotalKills = 0;
    }

    public void AddDamage(string weaponId, float dmg, bool killed)
    {
        if (string.IsNullOrEmpty(weaponId)) weaponId = "Unknown";
        _weaponList.TryGetValue(weaponId, out var s);
        s.damage += dmg;
        if (killed)
        {
            s.kills += 1;
            TotalKills += 1;
        }
        _weaponList[weaponId] = s;
    }

    public void AddExp(int gained)
    {
        if (gained <= 0) return;
        TotalExp += gained;
    }
}
