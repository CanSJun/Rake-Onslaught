using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardGameManager : MonoBehaviour, IGameInitializable
{
    [Header("UI")]
    [SerializeField] private CardController _cardUI;

    [Header("Offer")]
    [SerializeField] private int _offerCount = 3;

    [Header("Start Weapon")]
    [SerializeField] private string _startWeaponId;  

    private CardRuntimeManager _runtime;
    private CardService _service;
    private CardApplyContext _ctx;


    private readonly List<CardData> _candidates = new(128);
    private readonly CardData[] _offer = new CardData[3];

    private int _pendingLevelUps;
    private bool _isChoosing;
    public CardRuntimeManager Runtime => _runtime;

    void Awake()
    {
       
        _runtime = new CardRuntimeManager();
    }
    public void OnGameInitialized()
    {
        if (_cardUI != null) _cardUI.CloseInstant();

        
        var registry = CardEffectRegistry.CreateDefault();
        _service = new CardService(_runtime, registry);

        // 적용 대상 묶음
        _ctx = CardApplyContext.CreateCardContext();

        // 테스트용
        // _ctx.UnlockExistingWeapons(_runtime);
        // _ctx.SkillManager.UnlockAllSkills(_runtime);

        _runtime.UnlockWeapon(_startWeaponId);
        // 레벨업 이벤트 구독
        var exp = GameManager.Instance.PlayerExpManager;
        exp.OnLevelUp -= OnLevelUp;
        exp.OnLevelUp += OnLevelUp;
    }
    private void OnDestroy()
    {
        var gm = GameManager.Instance;
        if (gm != null && gm.PlayerExpManager != null)
            gm.PlayerExpManager.OnLevelUp -= OnLevelUp;
        // 혹시 카드 선택 중 파괴되면 강제 복구
        if (_isChoosing) PauseManager.PauseClear();
    }
    private void OnLevelUp(int level)
    {
        _pendingLevelUps++;
        // 이미 카드 선택 중이면 대기열에 쌓기
        if (_isChoosing) return;
        OpenOffer(true);
    }
    private void OpenOffer(bool firstOpen)
    {
        if (_cardUI == null) return;
        BuildCandidates();
        if (_candidates.Count <= 0)
        {
            // 후보가 없으면 그냥 종료
            _pendingLevelUps = 0;
            return;
        }

        PickOffer3Unique();

        _isChoosing = true;

        if (firstOpen) PauseManager.PushPause(showCursor: true);

        _cardUI.OpenOffer(_offer[0], _offer[1], _offer[2], OnSelected);
    }
    private void OnSelected(CardData selected)
    {
        if (selected != null) _service.ApplyCard(selected, _ctx);

        // 이번 레벨업 처리 완료
        _pendingLevelUps = Mathf.Max(0, _pendingLevelUps - 1);

        // 레벨업이 여러 번 연속으로 터졌으면 => 계속 카드 오픈
        if (_pendingLevelUps > 0)
        {
            OpenOffer(firstOpen: false); // pause 유지한 채 다음 카드
            return;
        }

        // 종료
        _isChoosing = false;
        _cardUI.CloseInstant();
        PauseManager.PopPause();
    }
    /// <summary>
    /// 등장 가능한 카드 후보를 DB에서 필터링
    /// </summary>
    private void BuildCandidates()
    {
        _candidates.Clear();

        var all = _cardUI.AllCards;
        if (all == null || all.Count == 0) return;

        for (int i = 0; i < all.Count; i++)
        {
            var card = all[i];
            if (card == null) continue;

            if (_service.CanAppear(card)) _candidates.Add(card);
        }
    }
    /// <summary>
    /// 후보에서 3장 고정 뽑기(중복 방지)
    /// </summary>
    private void PickOffer3Unique()
    {
        int n = _candidates.Count;

        if (n == 1)
        {
            //하나만 있으면 걍 모든거 똑같이
            _offer[0] = _candidates[0];
            _offer[1] = _candidates[0];
            _offer[2] = _candidates[0];
            return;
        }
        if (n == 2)
        {
            _offer[0] = _candidates[0];
            _offer[1] = _candidates[1];
            _offer[2] = _candidates[Random.Range(0, 2)];
            // 2개만 있으면 그냥 둘 중 하나만 마지막에 
            return;
        }

        // 3개 유니크: 뽑을 때마다 후보에서 제거(스왑 삭제)
        _offer[0] = PickOneWeightedAndRemove();
        _offer[1] = PickOneWeightedAndRemove();
        _offer[2] = PickOneWeightedAndRemove();
    }

    /// <summary>
    /// 가중치 기반 1장 선택 후 후보에서 제거
    /// - weight = card.weight * rarityWeight
    /// </summary>
    private CardData PickOneWeightedAndRemove()
    {
        float total = 0f;

        for (int i = 0; i < _candidates.Count; i++)
        {
            var c = _candidates[i];
            float w = Mathf.Max(0f, c.weight) * _runtime.RarityWeight(c.rarity);
            total += w;
        }

        int pickIndex = 0;

        if (total <= 0.0001f)   pickIndex = Random.Range(0, _candidates.Count); // 전부 0이면 균등 랜덤, 왜냐하면 이 상태에서 가중치 랜덤으로 하면 항상 첫 번째만 뽑히니깐!

        else
        {
            float r = Random.value * total;
            float acc = 0f; // 누적 가중치

            // 첫 번쨰 후보 acc = w0
            // 두 번째 후보 acc  = w0 + w1
            // 세 번째 후보 acc = w0 + w1 + w2
            
            for (int i = 0; i < _candidates.Count; i++)
            {
                var c = _candidates[i];
                float w = Mathf.Max(0f, c.weight) * _runtime.RarityWeight(c.rarity);
                acc += w;

                if (r <= acc) // acc안에 들어오면 해당하는 후보가 선택이 됨
                {
                    pickIndex = i;
                    break;
                }
            }
        }

        var picked = _candidates[pickIndex];

        int last = _candidates.Count - 1;
        _candidates[pickIndex] = _candidates[last];
        _candidates.RemoveAt(last);

        return picked;
    }
}