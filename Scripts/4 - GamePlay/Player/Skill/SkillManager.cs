using System.Collections.Generic;
using UnityEngine;

public  class SkillManager
{
    private GameManager _gameManager;
    private readonly SkillInputManager _inputManager;
    private readonly SkillRegistry _registry;
    private readonly Transform _caster;
    private readonly Dictionary<SkillSlot, SkillScriptableObject> _slotData;

    private readonly Dictionary<SkillSlot, int> _slotLevelData;

    private readonly float[] _cooldownRemaining;
    private readonly float[] _cooldownDuration;

    private CardRuntimeManager _runtime; 

    public SkillManager( SkillInputManager input, SkillRegistry registry, Dictionary<SkillSlot, SkillScriptableObject> slotData, Transform caster)
    {
        _inputManager = input;
        _registry = registry;
        _slotData = slotData;
        _caster = caster;


        _slotLevelData = new Dictionary<SkillSlot, int> {
            { SkillSlot.Skill1, 1 },
            { SkillSlot.Skill2, 1 },
            { SkillSlot.Skill3, 1 },
        };

        int slotCount = System.Enum.GetValues(typeof(SkillSlot)).Length;
        _cooldownRemaining = new float[slotCount];
        _cooldownDuration = new float[slotCount];

        // 기본 쿨타임 적용
        foreach (var pair in _slotData)
        {
            int idx = (int)pair.Key;
            if (idx < 0 || idx >= slotCount) continue; // 없으면 X
            _cooldownDuration[idx] = Mathf.Max(0f, pair.Value != null ? pair.Value.coolDown : 0f);
        }
        _gameManager = GameManager.Instance;
        _runtime = _gameManager.CardGameManager.Runtime;
    }

    public void Tick()
    {
        UpdateCooldown(Time.deltaTime);
        SyncCooldownUI();
        if (!_inputManager.TryRead(out var input)) return;
        if (!input.IsPressed) return;
        var slot = input.Slot;
        int idx = (int)slot;
        if (idx < 0 || idx >= _cooldownRemaining.Length) return;
        if (_cooldownRemaining[idx] > 0f)  return;  // 쿨타임
        var skill = _registry.GetSkill(slot);
        var data = _slotData[slot];
        if (data == null) return;
        if(_runtime == null || !_runtime.IsSkillUnlocked(data.skillID)) return; // 해금이 안됨
        int level = _slotLevelData.TryGetValue(slot, out var lev) ? lev : 1; // 만약에 그 레벨이 없으면 1로.. 안전하게
        var context = new SkillContext(data, _caster, level);

        skill.Execute(context);
        StartCooldown(slot, data.coolDown);
    }

    private void UpdateCooldown(float dt)
    {
        if (dt <= 0f) return;
        for (int i = 0; i < _cooldownRemaining.Length; i++)
        {
            if (_cooldownRemaining[i] <= 0f) continue;
            _cooldownRemaining[i] -= dt;
            if (_cooldownRemaining[i] < 0f) _cooldownRemaining[i] = 0f;
        }
    }

    private void StartCooldown(SkillSlot slot, float duration)
    {
        int idx = (int)slot;
        if (idx < 0 || idx >= _cooldownRemaining.Length) return;

        float cooldown = Mathf.Max(0f, duration);
        _cooldownDuration[idx] = cooldown;
        _cooldownRemaining[idx] = cooldown;
        _gameManager.SkillSlotManager.SetCooldown(slot, cooldown, cooldown);
    }
    public void UnlockAllSkills(CardRuntimeManager runtime)
    {
        if (runtime == null) return;
        foreach (var kv in _slotData)
        {
            var data = kv.Value;
            if (data == null) continue;
            if (!string.IsNullOrEmpty(data.skillID)) runtime.UnlockSkill(data.skillID);
        }
    }
    private void SyncCooldownUI()
    {

        for (int i = 0; i < _cooldownRemaining.Length; i++)
        {
            var slot = (SkillSlot)i;

            // duration이 0인데 data가 있으면 한번 갱신
            if (_cooldownDuration[i] <= 0f && _slotData.TryGetValue(slot, out var data) && data != null) _cooldownDuration[i] = Mathf.Max(0f, data.coolDown);

            _gameManager.SkillSlotManager.SetCooldown(slot, _cooldownRemaining[i], _cooldownDuration[i]);
        }
    }
    public bool ApplySkillLevelUp(string skillId, int delta)
    {
        if (string.IsNullOrEmpty(skillId) || delta == 0) return false;

        foreach (var kv in _slotData)
        {
            var data = kv.Value;
            if (data == null) continue;
            if (!string.Equals(data.skillID, skillId, System.StringComparison.Ordinal)) continue;

            return TryAddLevelBySlot(kv.Key, delta);
        }
        return false;
    }
    private bool TryAddLevelBySlot(SkillSlot slot, int delta)
    {
        int cur = _slotLevelData.TryGetValue(slot, out var lv) ? lv : 1;
        int next = cur + delta;
        if (next < 1) next = 1;

        if (next == cur) return false;
        _slotLevelData[slot] = next;
        return true;
    }
}
