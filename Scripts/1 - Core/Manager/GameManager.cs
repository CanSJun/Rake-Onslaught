
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static Util;

[DisallowMultipleComponent]
public class GameManager : MonoSingletonManager<GameManager>
{
    [Header("Managers")]
    [SerializeField, GetComponents("PoolManager")] private PoolManager _poolManager;
    [SerializeField, GetComponents("MonsterViewManager")] private MonsterViewManager _monsterViewManager;
    [SerializeField, GetComponents("BulletManager")] private BulletManager _bulletManager;
    [SerializeField, GetComponents("ExpDropManager")] private ExpDropManager _expDropManager;
    [SerializeField, GetComponents("CardManager")] private CardGameManager _cardGameManager;
    [SerializeField, GetComponents("DeathManager")] private DeathManager _deathManager;

    [Header("Skills")]
    [SerializeField] private SkillSlotView[] _skillSlotViews;
    [Header("WeaponSlots")]
    [SerializeField] private List<Image> _lockIcons;
    [Header("Pool")]
    [SerializeField] private PoolConfig _poolConfig;
    [Header("Player")]
    [SerializeField] GameObject _player;

    [Header("Canvas")]
    [SerializeField] private RectTransform _uiRectTransfrom;
    [Header("WeaponSlotsManager")]
    [SerializeField] TextMeshProUGUI _ammoText;
    [SerializeField] Image _weaponImage;
    [SerializeField] List<Image> _slots = new();
    [SerializeField] Color _unUsedColor;

    [Header("TextManager")]
    [SerializeField] private GameObject _damageTextPrefab;


    [Header("Difficulty Scaling")]
    [SerializeField] private float _hpGrowthPerMinute = 0.15f;     // 분당 HP +15%
    [SerializeField] private float _damageGrowthPerMinute = 0.12f; // 분당 데미지 +12%
    [SerializeField] private float _speedGrowthPerMinute = 0.05f;  // 분당 속도 +5%
    [SerializeField] private float _speedCapMultiplier = 2.0f;     // 속도 최대 2배
    [Header("Time")]
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    

    private int _lastShownSecond = -1;

    public float ElapsedTime { get; private set; } // 게임 시작 후 경과 시간(초)
    public float HpMult { get; private set; } = 1f;
    public float DamageMult { get; private set; } = 1f;
    public float SpeedMult { get; private set; } = 1f;
    public GameStats Stats { get; private set; }

    public bool IsGameOver { get; private set; }

    private TextManager _textManager;
    private WeaponSlotManager _weaponSlotManager;
    private BloodGroundManager _bloodGroundManager;
    private AnimationManager _animationManager;
    private SkillSlotManager _skillSlotManager;
    private PlayerExpManager _playerExpManager;
    private GameStats _gameStats;

    public PoolManager PoolManager => _poolManager;
    public WeaponSlotManager WeaponSlotManager => _weaponSlotManager;
    public TextManager TextManager => _textManager;
    public BloodGroundManager BloodGroundManager => _bloodGroundManager;
    public AnimationManager AnimationManager => _animationManager;

    public MonsterViewManager MonsterViewManager => _monsterViewManager;
    public BulletManager BulletManager => _bulletManager;

    public SkillSlotManager SkillSlotManager => _skillSlotManager;

    public PlayerExpManager PlayerExpManager => _playerExpManager;

    public ExpDropManager ExpDropManager => _expDropManager;

    public GameStats GameStats => _gameStats;

    private List<IGameInitializable> _initializables = new();

    private Animator _animator;

    public Animator PlayerAnimator => _animator;

    public DeathManager DeathManager => _deathManager;
    public CardGameManager CardGameManager => _cardGameManager;
    private void Awake()
    {
        base.Awake();
        Util.ApplyComponents(this);

        _weaponSlotManager = new WeaponSlotManager();
        _textManager = new TextManager();
        _bloodGroundManager = new BloodGroundManager();
        _animationManager = new AnimationManager();
        _skillSlotManager = new SkillSlotManager(); 
        _playerExpManager = new PlayerExpManager();
        _gameStats = new GameStats();
        var monos = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for(int i = 0; i < monos.Length; i++)
        {
            if (monos[i] is IGameInitializable init) _initializables.Add(init);
        }

    }

  

     

    private void FirstOn()
    {
        _textMeshProUGUI.enabled = true;

        _weaponSlotManager.Initialize(_ammoText, _weaponImage, _slots, _unUsedColor, _lockIcons);
        _textManager.Initialize(_damageTextPrefab, _uiRectTransfrom);
        _skillSlotManager.Initialize(_skillSlotViews);

        _playerExpManager.Reset();
        _player.GetComponent<PlayerController>().SetUI();
        _gameStats.Reset();


    }
    private async void Start()
    {
        _animator = _player.GetComponent<Animator>();

        await PoolManager.InitializeAsync(_poolConfig, null);
        for (int i = 0; i < _initializables.Count; i++) _initializables[i].OnGameInitialized();
        FirstOn();
    }

    private void UpdateTimeText()
    {
        int totalSeconds = Mathf.FloorToInt(ElapsedTime);
        if (totalSeconds == _lastShownSecond) return;
        _lastShownSecond = totalSeconds;
        int min = totalSeconds / 60;
        int sec = totalSeconds % 60;
        _textMeshProUGUI.SetText("{0:00}:{1:00}", min, sec);
    }
    private void Update()
    {
        if (START == 0) return;
        ElapsedTime += Time.deltaTime;

        float minutes = ElapsedTime / 60f;

        HpMult = 1f + minutes * _hpGrowthPerMinute;
        DamageMult = 1f + minutes * _damageGrowthPerMinute;
        SpeedMult = Mathf.Min(1f + minutes * _speedGrowthPerMinute, _speedCapMultiplier);
        UpdateTimeText();
    }


    public void OnPlayerDead()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        PauseManager.PushPause(showCursor: true);
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        _animationManager.SetTrigger(_animator, AnimationManager.DIE);
        _deathManager.Play(_gameStats, ElapsedTime);

    }



    public void GameQuit(string scene) {

        PauseManager.PauseClear();
        ResetStart();
        _poolManager.DestroyAllPools();

        var go = gameObject;
        Destroy(go);
        SceneManager.LoadScene(scene);
    }

}
