using UnityEngine;

public enum CardCategory 
{ 
    Skill, 
    Weapon,
    Passive
}
public enum CardRarity {
    Common, 
    Rare, 
    Epic,
    Legend
}
public enum CardEffectType
{
    SkillLevel,          // (slot) +1
    WeaponDamage,       // (weapon) +%
    FireRateDown,           // (weapon) -%
    FlameDPS,
    FlameRange,
    LaserTick,    // -%
    MoveSpeed,
    ExpMagnet,
    ExpGain,
    Jump,

    ReloadTimeDown,
    UnlockWeapon,
    UnlockSkill

}

// 스킬 레벨 업 증가
// 무기 데미지 증가
// 발사 속도 감소
// 화염 방사기 DPS 증가
// 화염 방사기 범위 증가
// 레이저 틱 데미지 증가
// 이동 속도 증가
// 경험치 자석 범위 증가
// 이동속도 증가
// 경험치 양 증가
// 점프 증가


[System.Serializable]
public struct CardEffect
{
    public CardEffectType type;
    public string targetId;     
    public float value;         
    public int intValue;       
}


[CreateAssetMenu(menuName = "Card")]
public class CardData : ScriptableObject
{
    [Header("UI")]
    public string title;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Setting")]
    public string cardId;        
    public CardCategory category;
    public CardRarity rarity;

    public int maxLevel;       
    public float weight;        // 출현 가중치

    public string[] requiredCardIds; // 선행 조건
    public CardEffect[] effects;     // 실제 적용 효과

}