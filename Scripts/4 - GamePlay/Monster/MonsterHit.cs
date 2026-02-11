
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UIElements;
using System;
public class MonsterHit 
{
    private Animator _animator;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    private float _damage;
    private int _damageCheck;
    private int _damageDelay;
    private CancellationToken _lifeToken;

    private bool _isHit;
    private AnimationManager _animManager;
    private int Hithash;
    private int _actionDelay;

    private GameManager _gamemanager;
    private readonly Color _hitColor = new Color(1f, 0.3f, 0.3f);
    private MaterialPropertyBlock _hitMaterialPropertyBlock; // Material은 그대로 두고 Renderer에만 덮어쓸 값을 잠시만 줌 => Material 복제가 없음 
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private Tween _hitTween;
    public void Initialize(Animator animator, SkinnedMeshRenderer skinnedMeshRenderer, int damageDelay, CancellationToken token, int actionDelay)
    {
        _animator = animator;
        _skinnedMeshRenderer = skinnedMeshRenderer;
        _damageDelay = damageDelay;
        _lifeToken = token;
        _isHit = false;
        _animManager = GameManager.Instance.AnimationManager;
        Hithash = AnimationManager.HIT;
        _actionDelay = actionDelay;
        _hitMaterialPropertyBlock ??= new MaterialPropertyBlock();
        _hitMaterialPropertyBlock.Clear();
        _skinnedMeshRenderer.SetPropertyBlock(_hitMaterialPropertyBlock);
        _damageCheck = 0;
        _isHit = false;
        _hitTween?.Kill();
        _hitTween = null;
        _gamemanager = GameManager.Instance;
    }

    public void CleanUP()
    {
        _hitTween?.Kill();
        _hitTween = null;
        _damage = 0f;
        _damageCheck = 0;
        _isHit = false;
        _hitMaterialPropertyBlock.Clear();
        _skinnedMeshRenderer.SetPropertyBlock(_hitMaterialPropertyBlock);
        
    }

    /// <summary>
    /// 몬스터 피해 함수
    /// </summary>
    public void OnDamaged(float damage, Vector3 position, bool c)
    {

        _damage += damage;
        _damageCheck++;
        OnDamageTextSpawn(position, _damageCheck).Forget(); // 기다리지 않음

        HitAimation();


    }

    /// <summary>
    /// 텍스트 소환 주기
    /// </summary>
    private async UniTaskVoid OnDamageTextSpawn(Vector3 worldPos, int check)
    {
        try
        {
            await UniTask.Delay(_damageDelay, DelayType.DeltaTime, PlayerLoopTiming.Update, _lifeToken);
            if (check != _damageCheck) return; // 최신의 공격이 있으면 이전 기록 무시 

            _gamemanager.TextManager.DamageTextSpawn(_damage, worldPos);
            _damage = 0;
            
        }
        catch (OperationCanceledException)
        {
             // Disable 시 
        }
    }


    /// <summary>
    /// Hit Animation 주기
    /// </summary>
    
    private async UniTaskVoid OnHitCoolDown()
    {
        try
        {
            await UniTask.Delay(_actionDelay, DelayType.DeltaTime, PlayerLoopTiming.Update, _lifeToken);
        }
        catch (OperationCanceledException)
        {
            // Disable 시 
        }

        _isHit = false;
    }
    private void HitAimation()
    {
        if (_isHit) return;
        _isHit = true;
        _hitTween?.Kill();

        //DOvirtual from to duration onValue => 시작값, 끝값, 걸리는 시간, 매 프레임 호출되는 callback
        _hitTween = DOVirtual.Float(1f, 0f, 0.15f, value =>
        {
            Color color = Color.Lerp(_hitColor, Color.white, value);
            _hitMaterialPropertyBlock.SetColor(BaseColor, color);
            _skinnedMeshRenderer.SetPropertyBlock(_hitMaterialPropertyBlock);
        }).SetEase(Ease.OutQuad).OnComplete(() =>
        {

            _hitMaterialPropertyBlock.Clear();
            _skinnedMeshRenderer.SetPropertyBlock(_hitMaterialPropertyBlock);
        });

    _animManager.SetTrigger(_animator, Hithash);
        OnHitCoolDown().Forget();

    }


}
