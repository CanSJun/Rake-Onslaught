using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


public class MainScript : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;
    private AudioSource _BGM;
    void Start()
    {

    }

    public void ChangeVolume() => _BGM.volume = PlayerPrefs.GetFloat("Music_Volume", 100) / 100f * PlayerPrefs.GetFloat("Master_Volume", 100) / 100f;
}
