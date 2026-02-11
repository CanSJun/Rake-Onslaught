using UnityEngine;
using UnityEngine.Audio;



public enum SfxId : int
{
    None = 0,

    // 총 소리
    AutoGunShot = 1,
    ShotGunShot = 2,
    //FlameGunShot = 3, // 지속형은 따로 관리
    //LagerGunShot = 4,
    Reload = 5,

    // 스킬 소리
    MissileLaunch = 6,
    MissileImpact = 7,
    EMP = 8,
    OverDrive = 9,

    // 몬스터, 유저 
    RakeDie = 10,
    HitSound = 11,

    // UI
    ButtonClick = 12,
    CardAppear = 13,
    CardMove = 14,
    CardSelect = 15,

    Count // 몇개인지 체크
}
public enum BGM : int
{
    Main,
    InGame,
    Result,
    Count // 몇개인지 체크
}


[DisallowMultipleComponent]
public class SoundManager : MonoSingletonManager<SoundManager>
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private string masterParam = "MasterVol";
    [SerializeField] private string musicParam = "MusicVol";
    [SerializeField] private string sfxParam = "SfxVol";

    [Header("Clips")]
    [SerializeField] private AudioClip[] _bgmClips = new AudioClip[(int)BGM.Count];
    [SerializeField] private AudioClip[] _sfxClips = new AudioClip[(int)SfxId.Count];

    [Header("3D SFX Pool")]
    [SerializeField] private int _voiceCount = 24;
    [SerializeField] private float _defaultMinDistance = 1.5f;
    [SerializeField] private float _defaultMaxDistance = 25f;
    [SerializeField] private AudioRolloffMode _rolloff = AudioRolloffMode.Logarithmic;


    private const string PREF_MASTER = "Master_Volume";
    private const string PREF_MUSIC = "Music_Volume";
    private const string PREF_SFX = "SFX_Volume";

    private AudioSource _bgm;
    private AudioSource _uiSfx;
    private AudioSource[] _voices;
    private int _cursor;

    private float _master01 = 1f;
    private float _music01 = 1f;
    private float _sfx01 = 1f;

    private void Awake()
    {
        base.Awake();
        Setting();
        LoadPrefsAndApply();
    }
    private void Setting()
    {
        _bgm = gameObject.AddComponent<AudioSource>();
        _bgm.playOnAwake = false;
        _bgm.loop = true;
        _bgm.spatialBlend = 0f;
        _bgm.outputAudioMixerGroup = bgmGroup;

        _uiSfx = gameObject.AddComponent<AudioSource>();
        _uiSfx.playOnAwake = false;
        _uiSfx.loop = false;
        _uiSfx.spatialBlend = 0f;
        _uiSfx.outputAudioMixerGroup = sfxGroup;

        // 거리감쇠
        if (_voiceCount < 8) _voiceCount = 8;
        _voices = new AudioSource[_voiceCount];

        var root = new GameObject("Sfx3DPool").transform;
        root.SetParent(transform, false);

        for (int i = 0; i < _voiceCount; i++)
        {
            var go = new GameObject($"Sfx3D_{i}");
            go.transform.SetParent(root, false);

            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = false;

            s.spatialBlend = 1f;                
            s.rolloffMode = _rolloff;
            s.minDistance = _defaultMinDistance;
            s.maxDistance = _defaultMaxDistance;
            s.dopplerLevel = 0f;

            if (sfxGroup) s.outputAudioMixerGroup = sfxGroup;

            _voices[i] = s;
        }
    }

    public void LoadPrefsAndApply()
    {
        _master01 = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, 100f) / 100f);
        _music01 = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, 100f) / 100f);
        _sfx01 = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_SFX, 100f) / 100f);
        ApplyVolumes();
    }

    public void SetMaster01(float v) { _master01 = Mathf.Clamp01(v); ApplyVolumes(); }
    public void SetMusic01(float v) { _music01 = Mathf.Clamp01(v); ApplyVolumes(); }
    public void SetSfx01(float v) { _sfx01 = Mathf.Clamp01(v); ApplyVolumes(); }

    private void ApplyVolumes()
    {
        if (mixer != null)
        {
            mixer.SetFloat(masterParam, LinToDb(_master01));
            mixer.SetFloat(musicParam, LinToDb(_music01));
            mixer.SetFloat(sfxParam, LinToDb(_sfx01));
        }
        else
        {
            // Mixer 안 쓰면 수동 곱으로 처리
            _bgm.volume = _master01 * _music01;
            _uiSfx.volume = _master01 * _sfx01;
        }
    }

    private static float LinToDb(float v01)=> (v01 <= 0.0001f) ? -80f : Mathf.Log10(v01) * 20f;

    public void PlayBgm(BGM id, bool restart = false)
    {
        var clip = GetBgm(id);
        if (clip == null) return;

        if (!restart && _bgm.isPlaying && _bgm.clip == clip) return;

        _bgm.clip = clip;
        _bgm.Play();
    }

    public void StopBgm() => _bgm.Stop();


    public void PlaySfx(SfxId id, float volume01 = 1f, float pitch = 1f)
    {

        var clip = GetSfx(id);
        Debug.Log(clip.name);
        if (clip == null) return; 

        _uiSfx.pitch = pitch;

        float v = Mathf.Clamp01(volume01);
        if (mixer == null) v *= (_master01 * _sfx01);


        _uiSfx.PlayOneShot(clip, v);
    }


    public void PlaySfxAt(
        SfxId id,
        Vector3 pos,
        float volume01 = 1f,
        float pitch = 1f,
        float? minDist = null,
        float? maxDist = null)
    {
        var clip = GetSfx(id);
        if (clip == null) return;

        var s = NextVoice();

        s.transform.position = pos;
        s.minDistance = minDist ?? _defaultMinDistance;
        s.maxDistance = maxDist ?? _defaultMaxDistance;
        s.pitch = pitch;

        float v = Mathf.Clamp01(volume01);
        if (mixer == null) v *= (_master01 * _sfx01);
        s.volume = v;

        // 단순하게: 현재 재생 중이면 끊고 새로 재생 (가장 직관적)
        s.Stop();
        s.clip = clip;
        s.Play();
    }

    private AudioSource NextVoice()
    {
        // 제일 단순한 라운드로빈
        int idx = _cursor;
        _cursor = (_cursor + 1) % _voices.Length;
        return _voices[idx];
    }

    private AudioClip GetSfx(SfxId id)
    {
        int i = (int)id;
        if (i < 0 || i >= _sfxClips.Length) return null;
        return _sfxClips[i];
    }

    private AudioClip GetBgm(BGM id)
    {
        int i = (int)id;
        if (i < 0 || i >= _bgmClips.Length) return null;
        return _bgmClips[i];
    }
}