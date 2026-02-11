using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using static Util;
public class CameraScript : MonoBehaviour
{
    public Transform _target;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private Vector3 _overDriveOffset;
    [SerializeField] private float _offsetSharpness;
   // [SerializeField] private float _speed = 0.125f;
    [SerializeField] private float _snapDistance = 6f;
    [SerializeField] private float _follow = 12f;
    [SerializeField] GameObject _cinematic;

    [Header("Shake")]
    [GetComponents("ApplyEffect")] public Transform _shakeTransform;
    [SerializeField] private float _shakeTime;
    [SerializeField] private float _shakePower;

    private Vector3 _originPosition;
    private Transform _transfrom;


    private Vector3 _offsetTarget;

    [GetComponents("Main Camera")] public Camera _camera;
    private void Awake()
    {
        Util.ApplyComponents(this);
    }

    private void Start()
    {
        Util.CameraScriptSetting(this);
    }

    private void LateUpdate()
    {
        if (_target == null || START == 0 || PauseManager.IsPaused) return;
        float ot = 1f - Mathf.Exp(-_offsetSharpness * Time.deltaTime);
        Vector3 offset = Vector3.Lerp(_offset, _offsetTarget, ot);
        Vector3 desired = _target.position + _offset;

        float dist = Vector3.Distance(_transfrom.position, desired);
        if (dist > _snapDistance) _transfrom.position = desired;
        else
        {
            float t = 1f - Mathf.Exp(-_follow * Time.deltaTime);
            _transfrom.position = Vector3.Lerp(_transfrom.position, desired, t);
        }
        _transfrom.LookAt(_target);

        if(_shakeTime <= 0f) _shakeTransform.localPosition = _originPosition;
        else
        {
            _shakeTime -= Time.deltaTime;
            _shakeTransform.localPosition = _originPosition + Random.insideUnitSphere * _shakePower;
            
        }
    }

    public void SetShake(float duration, float power)
    {
        _shakePower = power;
        _shakeTime = duration;
    }
    public void SetOverDriveZoom(bool on) => _offsetTarget = on ? _overDriveOffset : _offset;

    public void Setting()
    {

        _cinematic.gameObject.SetActive(false);
        _camera.transform.localPosition = Vector3.zero;
        _camera.transform.localRotation = Quaternion.identity;
        _originPosition = _shakeTransform.localPosition;
        _transfrom = transform;
        _offsetTarget = _offset;
        _transfrom.LookAt(_target);
        OnStart();
    }
}
