using System.Collections;
using UnityEngine;

public class DeathManager : MonoBehaviour, IGameInitializable
{

    

    [Header("Overlay")]
    [SerializeField] private CanvasGroup _overlay;

    [Header("Player Only Camera")]
    [SerializeField] private Camera _playerOnlyCamera; // Player 레이어만 렌더

    [Header("Report")]
    [SerializeField] private ReportController _reportUI;

    [Header("Timing")]
    [SerializeField] private float _fadeInTime = 1.25f;
    [SerializeField] private float _reportDelay = 0.35f;

    [Header("Buttons")]

    [SerializeField] private string _mainScene = "MainScene";

    private Coroutine _corutine;
    private Camera _camera;
    private Animator _playerAnimator;
    private int DIE;
    private void Awake()
    {

        _overlay.alpha = 0f;
        _overlay.blocksRaycasts = false;
        _overlay.interactable = false;
        _playerOnlyCamera.enabled = false;
        _reportUI.Hide();
    }
    public void OnGameInitialized() {
        _playerAnimator = GameManager.Instance.PlayerAnimator;
        DIE = AnimationManager.DIE;
    }

    private void Start()
    {
        _camera = Camera.main;

    }

    private void LateUpdate()
    {
        // 플레이어 카메라가 메인 카메라를 따라가게
        if (_playerOnlyCamera && _playerOnlyCamera.enabled && _camera)
        {
            var transform = _playerOnlyCamera.transform;
            transform.position = _camera.transform.position;
            transform.rotation = _camera.transform.rotation;
        }
    }

    public void Play(GameStats stats, float elapsedTime)
    {
        if (_corutine != null) StopCoroutine(_corutine);
        _corutine = StartCoroutine(OnPopUp(stats, elapsedTime));
    }

    private IEnumerator OnPopUp(GameStats stats, float elapsedTime)
    {
        if (_reportUI) _reportUI.Hide(); // 만약을 위해서 처음엔 Hide
        if (_playerOnlyCamera) _playerOnlyCamera.enabled = true;



        float timeoutAt = Time.unscaledTime + 1f;
        bool entered = false;

        // 1) DIE 상태 진입 대기
        while (Time.unscaledTime < timeoutAt)
        {
            var st = _playerAnimator.GetCurrentAnimatorStateInfo(0);
            if (st.shortNameHash == DIE) { entered = true; break; }
            yield return null;
        }

        // 2) DIE 끝까지 대기
        if (entered)
        {
            while (true)
            {
                var st = _playerAnimator.GetCurrentAnimatorStateInfo(0);

                if (st.shortNameHash != DIE) break;   
                if (st.normalizedTime >= 1f) break;  

                yield return null;
            }
        }


        // 오버레이 페이드 인
        if (_overlay)
        {
            _overlay.blocksRaycasts = true;

            float t = 0f;
            while (t < _fadeInTime)
            {
                t += Time.unscaledDeltaTime;
                _overlay.alpha = Mathf.Clamp01(t / _fadeInTime);
                yield return null;
            }
            _overlay.alpha = 1f;
        }
        // 약간 텀 주고 리포트 띄우기
        float delay = _reportDelay;
        while (delay > 0f)
        {
            delay -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (_reportUI) _reportUI.Show(stats, elapsedTime);
    }



    public void OnClickExit() => GameManager.Instance.GameQuit(_mainScene);

}