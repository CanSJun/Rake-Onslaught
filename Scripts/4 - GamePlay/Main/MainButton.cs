using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainButton : MonoBehaviour
{

    [SerializeField] private GameObject _Option;


    public void OnStartButton() { ButtonEffect();  Loader.Load("GameScene"); }

    public void OnOptionButton() => CommonToggle(_Option, true);
    public void OnOptionOkButton() => CommonToggle(_Option, false);
    public void OnExitButton() { ButtonEffect(); Application.Quit(); }

    private void CommonToggle(GameObject gameObject, bool state)
    {
        if (gameObject != null) gameObject.SetActive(state);
        ButtonEffect();
    }
    public void ButtonEffect()
    {
        SoundManager.Instance.PlaySfx(SfxId.ButtonClick);
    }
}
