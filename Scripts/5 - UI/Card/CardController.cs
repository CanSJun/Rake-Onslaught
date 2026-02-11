using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.GPUSort;

public class CardController : MonoBehaviour
{
    [Header("All Card Data")]
    [SerializeField] private List<CardData> _cards = new();
    public List<CardData> AllCards => _cards;

    [Header("Views (Slots)")]
    [SerializeField] private CardView _leftView;
    [SerializeField] private CardView _centerView;
    [SerializeField] private CardView _rightView;

    [Header("Scales")]
    [SerializeField] private Vector3 _sideScale = new(0.8f, 0.8f, 1f);
    [SerializeField] private Vector3 _centerScale = new(1.15f, 1.15f, 1f);

    [Header("Animation")]
    [SerializeField] private float _animTime = 0.12f;
    [SerializeField] private float _inputRepeatLock = 0.12f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference _navigate;
    [SerializeField] private InputActionReference _submit;

    private readonly List<CardData> _offerCards = new(3);
    private Action<CardData> _onSelected;

    private int _currentIndex;

    private Vector2 _leftPos, _centerPos, _rightPos;

    private bool _isAnimating;
    private int _animDir;
    private float _anim01;
    private float _lockUntil;

    private Vector2 _lStartPos, _cStartPos, _rStartPos;
    private Vector2 _lTargetPos, _cTargetPos, _rTargetPos;

    private Vector3 _lStartScale, _cStartScale, _rStartScale;
    private Vector3 _lTargetScale, _cTargetScale, _rTargetScale;

    private RectTransform LRt => _leftView.Rect;
    private RectTransform CRt => _centerView.Rect;
    private RectTransform RRt => _rightView.Rect;

    private bool _inputBound;
    private bool _submittedOnce;

    private SoundManager _sound;
    private void Awake()
    {
        _leftPos = LRt.anchoredPosition;
        _centerPos = CRt.anchoredPosition;
        _rightPos = RRt.anchoredPosition;
        _sound = SoundManager.Instance;
        CloseInstant();
    }
    public void OpenOffer(CardData a, CardData b, CardData c, Action<CardData> onSelected)
    {
        gameObject.SetActive(true);

        _offerCards.Clear();
        if (a != null) _offerCards.Add(a);
        if (b != null) _offerCards.Add(b);
        if (c != null) _offerCards.Add(c);

        if (_offerCards.Count == 0)
        {
            CloseInstant();
            return;
        }

        _sound.PlaySfx(SfxId.CardAppear);
        _submittedOnce = false;
        _onSelected = onSelected;
        _currentIndex = 0;

        BindAll();
        SnapToSlotPose();
        ApplyRoleVisuals();

        BindInput(true);
    }
    public void CloseInstant()
    {
        BindInput(false);
        _offerCards.Clear();
        _onSelected = null;
        _submittedOnce = false;
        _isAnimating = false;
        gameObject.SetActive(false);
    }
    private void BindInput(bool enable)
    {
        if (_navigate == null || _submit == null) return;

        if (enable)
        {
            if (_inputBound) return;

            _navigate.action.Enable();
            _submit.action.Enable();
            _navigate.action.performed += OnNavigate;
            _submit.action.performed += OnSubmit;

            _inputBound = true;
        }
        else
        {
            if (!_inputBound) return;

            _navigate.action.performed -= OnNavigate;
            _submit.action.performed -= OnSubmit;
            _navigate.action.Disable();
            _submit.action.Disable();

            _inputBound = false;
        }
    }

    private float Smooth01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }

    private  int Wrap(int i, int count)
    {
        if (count <= 0) return 0;
        i %= count;
        if (i < 0) i += count;
        return i;
    }

    private void Update()
    {
        if (!_isAnimating) return;

        float dt = Time.unscaledDeltaTime;
        _anim01 += dt / Mathf.Max(0.0001f, _animTime);
        float t = Smooth01(_anim01);

        // 위치
        LRt.anchoredPosition = Vector2.LerpUnclamped(_lStartPos, _lTargetPos, t);
        CRt.anchoredPosition = Vector2.LerpUnclamped(_cStartPos, _cTargetPos, t);
        RRt.anchoredPosition = Vector2.LerpUnclamped(_rStartPos, _rTargetPos, t);

        // 스케일
        LRt.localScale = Vector3.LerpUnclamped(_lStartScale, _lTargetScale, t);
        CRt.localScale = Vector3.LerpUnclamped(_cStartScale, _cTargetScale, t);
        RRt.localScale = Vector3.LerpUnclamped(_rStartScale, _rTargetScale, t);

        if (_anim01 >= 1f) FinishMove();
    }

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        int count = _offerCards.Count;
        if (count <= 1) return;
        if (_isAnimating) return;
        // 입력 락 (연타 방지)
        if (Time.unscaledTime < _lockUntil) return;

        _sound.PlaySfx(SfxId.CardMove);
        Vector2 v = ctx.ReadValue<Vector2>();
        if (v.x <= -0.5f) BeginMove(-1);
        else if (v.x >= 0.5f) BeginMove(+1);
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (_offerCards.Count <= 0) return;
        if (_submittedOnce) return; // 중복 클릭 방지

        _sound.PlaySfx(SfxId.CardSelect);
        _submittedOnce = true;
        BindInput(false);
        var selected = _offerCards[_currentIndex];
        _onSelected?.Invoke(selected);
    }

    private void BeginMove(int dir)
    {
        
        
        _lockUntil = Time.unscaledTime + _inputRepeatLock;
        _animDir = dir;
        _anim01 = 0f;
        _isAnimating = true;

        // 선택 인덱스 갱신
        _currentIndex = Wrap(_currentIndex + dir, _offerCards.Count);

        // 시작 상태 
        _lStartPos = LRt.anchoredPosition;
        _cStartPos = CRt.anchoredPosition;
        _rStartPos = RRt.anchoredPosition;

        _lStartScale = LRt.localScale;
        _cStartScale = CRt.localScale;
        _rStartScale = RRt.localScale;

        // 목표 슬롯
        if (dir > 0)
        {
            // left(재활용 후보) -> rightPos, center -> leftPos, right -> centerPos
            _lTargetPos = _rightPos;
            _cTargetPos = _leftPos;
            _rTargetPos = _centerPos;

            _lTargetScale = _sideScale;
            _cTargetScale = _sideScale;
            _rTargetScale = _centerScale; // right가 센터로 들어오며 커짐
        }
        else
        {
            // left -> centerPos, center -> rightPos, right(재활용 후보) -> leftPos
            _lTargetPos = _centerPos;
            _cTargetPos = _rightPos;
            _rTargetPos = _leftPos;

            _lTargetScale = _centerScale; // left가 센터로 들어오며 커짐
            _cTargetScale = _sideScale;
            _rTargetScale = _sideScale;
        }
    }

    private void FinishMove()
    {
        // 목표값으로 스냅
        LRt.anchoredPosition = _lTargetPos;
        CRt.anchoredPosition = _cTargetPos;
        RRt.anchoredPosition = _rTargetPos;

        LRt.localScale = _lTargetScale;
        CRt.localScale = _cTargetScale;
        RRt.localScale = _rTargetScale;

        _isAnimating = false;

        // 뷰 회전
        RotateViewsAndRebind(_animDir);

        // 정확한 슬롯 정렬(드리프트 방지)
        SnapToSlotPose();
        ApplyRoleVisuals();
    }

    private void RotateViewsAndRebind(int dir)
    {
        int count = _offerCards.Count;
        if (count <= 0) return;

        if (dir > 0)
        {
            // D: (L,C,R) -> (C,R,L)
            var oldL = _leftView;
            var oldC = _centerView;
            var oldR = _rightView;
            _leftView = oldC;
            _centerView = oldR;
            _rightView = oldL;

            // 재활용된 rightView(원래 left였던 카드)에 "새로운 오른쪽 카드" 바인딩
            int rightIndex = Wrap(_currentIndex + 1, count);
            _rightView.Bind(_offerCards[rightIndex]);
        }
        else
        {
            // A: (L,C,R) -> (R,L,C)
            var oldL = _leftView;
            var oldC = _centerView;
            var oldR = _rightView;

            _leftView = oldR;
            _centerView = oldL;
            _rightView = oldC;

            // 재활용된 leftView(원래 right였던 카드)에 "새로운 왼쪽 카드" 바인딩
            int leftIndex = Wrap(_currentIndex - 1, count);
            _leftView.Bind(_offerCards[leftIndex]);
        }

        // center는 항상 선택 카드를 줘야하니 한번 더 안전상으로!
        _centerView.Bind(_offerCards[_currentIndex]);
    }

    private void BindAll()
    {
        int count = _offerCards.Count;
        if (count <= 0) return;

        int leftIndex = Wrap(_currentIndex - 1, count);
        int rightIndex = Wrap(_currentIndex + 1, count);

        _leftView.Bind(_offerCards[leftIndex]);
        _centerView.Bind(_offerCards[_currentIndex]);
        _rightView.Bind(_offerCards[rightIndex]);
    }
    private void ApplyRoleVisuals()
    {
        _leftView.SetVisual(alpha: 0.55f, interactable: false);
        _rightView.SetVisual(alpha: 0.55f, interactable: false);
        _centerView.SetVisual(alpha: 1f, interactable: true);
    }

    private void SnapToSlotPose()
    {
        _leftView.Rect.anchoredPosition = _leftPos;
        _centerView.Rect.anchoredPosition = _centerPos;
        _rightView.Rect.anchoredPosition = _rightPos;

        _leftView.Rect.localScale = _sideScale;
        _centerView.Rect.localScale = _centerScale;
        _rightView.Rect.localScale = _sideScale;
    }



}