using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;
public class CrossHairScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _player;

    private Camera _camera;
    private IInput _input;
    [Header("Settting")]
    [SerializeField] private float _maxDistance = 5f;
    [SerializeField] private float _smoothSpeed = 10f;



    private RectTransform _rectTransfrom;
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _rectTransfrom = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _camera = Camera.main;
        _input = new PlayerInput();
    }

    private void LateUpdate()
    {
        if (PauseManager.IsPaused || START == 0 ) return;
        if ( _player == null || _camera == null) return;

        if (_input.IsAim == false) { _canvasGroup.alpha = 0f; return; }
        else _canvasGroup.alpha = 1f;


        Vector3 mousePos = _input.MousePosition;
        mousePos.z = 10;
        Vector3 target = _camera.ScreenToWorldPoint(mousePos);
        Vector3 distance = target - _player.position;

        if (distance.magnitude > _maxDistance) distance = distance.normalized * _maxDistance;
        Vector3 clamped = _player.position + distance;
        Vector3 newPos = _camera.WorldToScreenPoint(clamped);
        newPos.z = 0f;
        _rectTransfrom.position = Vector3.Lerp(_rectTransfrom.position, newPos, _smoothSpeed * Time.deltaTime);
    }

}
