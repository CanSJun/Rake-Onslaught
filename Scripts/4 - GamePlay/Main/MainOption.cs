using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainOption : MonoBehaviour
{
    [SerializeField] private Slider _master;
    [SerializeField] private TextMeshProUGUI _masterText;
    [SerializeField] private Slider _sfx;
    [SerializeField] private TextMeshProUGUI _sfxText;
    [SerializeField] private Slider _music;
    [SerializeField] private TextMeshProUGUI _musicText;




    private MainScript _mainScript;
    private const string MASTER = "Master_Volume";
    private const string SFX = "SFX_Volume";
    private const string MUSIC = "Music_Volume";
    private void Start()
    {
        _master.value = PlayerPrefs.GetFloat(MASTER, 100) / 100f;

        _sfx.value = PlayerPrefs.GetFloat(SFX, 100) / 100f;
        _music.value = PlayerPrefs.GetFloat(MUSIC, 100) / 100f;

        _masterText.SetText("{0}", Mathf.Floor(_master.value * 100));
        _sfxText.SetText("{0}", Mathf.Floor(_sfx.value * 100)   );
        _musicText.SetText("{0}", Mathf.Floor(_music.value * 100));

        _master.onValueChanged.AddListener(OnMasterChanged);
        _sfx.onValueChanged.AddListener(OnSFXChanged);
        _music.onValueChanged.AddListener(OnMusicChanged);

        _mainScript = GetComponent<MainScript>();
    }

    private void OnMasterChanged(float value)
    {
        _masterText.text = Mathf.Floor(value * 100).ToString();
        PlayerPrefs.SetFloat(MASTER, value * 100);
        PlayerPrefs.Save();

        SoundManager.Instance.SetMaster01(value); 
    }
    private void OnSFXChanged(float value)
    {
        _sfxText.text = Mathf.Floor(value * 100).ToString();
        PlayerPrefs.SetFloat(SFX, value * 100);
        PlayerPrefs.Save();
        SoundManager.Instance.SetSfx01(value);
    }
    private void OnMusicChanged(float value)
    {
        _musicText.text = Mathf.Floor(value * 100).ToString();
        PlayerPrefs.SetFloat(MUSIC, value * 100);
        PlayerPrefs.Save();
        _mainScript.ChangeVolume();
        SoundManager.Instance.SetMusic01(value);
    }


}
