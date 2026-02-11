using System;
using System.Collections.Generic;
using UnityEngine;
public class CardRuntimeManager
{
    private readonly Dictionary<string, int> _cardLevels = new();

    private readonly HashSet<string> _unlockedWeapons = new();
    private readonly HashSet<string> _unlockedSkills = new();

    public event Action<string> WeaponUnlocked;
    public event Action<string> SkillUnlocked;

    private readonly Dictionary<CardRarity, float> _rarityWeight = new()
    {
        { CardRarity.Common, 1.0f },
        { CardRarity.Rare,   0.35f },
        { CardRarity.Epic,   0.12f },
        { CardRarity.Legend,   0.05f }
    };

    public void Reset()
    {
        _cardLevels.Clear();
        _unlockedWeapons.Clear();
        _unlockedSkills.Clear();
    }
    public int GetLevel(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return 0;
        return _cardLevels.TryGetValue(cardId, out var level) ? level : 0;
    }

    public bool HasCard(string cardId) => GetLevel(cardId) > 0;
    public bool CanLevelUp(CardData card) => card != null && !string.IsNullOrEmpty(card.cardId) && GetLevel(card.cardId) < Mathf.Max(1, card.maxLevel);
    public float RarityWeight(CardRarity rarity) => _rarityWeight.TryGetValue(rarity, out var w) ? w : 1f;

    public void AddCardLevel(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return; _cardLevels[cardId] = GetLevel(cardId) + 1;
    }
    public void UnlockWeapon(string weaponId)
    {
        if (string.IsNullOrEmpty(weaponId)) return;
        if (_unlockedWeapons.Add(weaponId)) WeaponUnlocked?.Invoke(weaponId);
    }

    public void UnlockSkill(string skillId)
    {
        if (string.IsNullOrEmpty(skillId)) return;
        if (_unlockedSkills.Add(skillId)) SkillUnlocked?.Invoke(skillId);
    }

    public bool IsWeaponUnlocked(string weaponId) => string.IsNullOrEmpty(weaponId) || _unlockedWeapons.Contains(weaponId);
    public bool IsSkillUnlocked(string skillId) => string.IsNullOrEmpty(skillId) || _unlockedSkills.Contains(skillId);

}