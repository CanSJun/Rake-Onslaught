using System.Collections.Generic;
using UnityEngine;

public class ExpDropManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform _player;


    [Header("Option")]
    [SerializeField, Tooltip("자석 거리")] private float _magnetRadius = 6f; 
    [SerializeField, Tooltip("먹는 거리")] private float _pickupRadius = 1.2f;   
    [SerializeField, Tooltip("빨려오는 속도")] private float _magnetSpeed = 12f;  
    [SerializeField, Tooltip("드랍 시에 바닥에 붙이기")] private float _groundRayHeight = 5f;
    [SerializeField, Tooltip("프레임 제한")] private int _maxUpdatePerFrame = 256;


    private readonly List<ExpBallScript> _active = new();
    private int _cursor;
    private GameManager _gameManager;

    private bool _cachedMagnetBase;
    private float _baseMagnetRadius;
    private float _magnetMul = 1f;


    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    private void CacheMagnetBase()
    {
        if (_cachedMagnetBase) return;
        _baseMagnetRadius = _magnetRadius;
        _cachedMagnetBase = true;
    }
    public void ApplyMagnetRadiusMultiplier(float mul)
    {
        CacheMagnetBase();
        if (mul <= 0f) mul = 0.01f;
        _magnetMul *= mul;
        _magnetRadius = _baseMagnetRadius * _magnetMul;
    }

    private int Chunk(int remain)
    {
        if (remain >= 25) return 25;
        if (remain >= 10) return 10;
        if (remain >= 5) return 5;
        return 1;
    }
    private Vector3 GetGroundPos(Vector3 pos)
    {
        if (Physics.Raycast(pos + Vector3.up * _groundRayHeight, Vector3.down, out var hit, 100f)) pos.y = hit.point.y + 0.05f;
        return pos;
    }


    public void Drop(Vector3 worldPos, int totalExp)
    {

        while(totalExp > 0)
        {
            int chunk = Chunk(totalExp);
            totalExp -= chunk;

            Vector3 pos = GetGroundPos(worldPos + Random.insideUnitSphere * 1.0f);

            var obj = _gameManager.PoolManager.Spawn(PoolKeys.ExpBall);
            var ball = obj.GetComponent<ExpBallScript>();
            pos = GetGroundPos(pos);

            ball.Activate(chunk, pos);
            _active.Add(ball);
        }
    }

    private void Update()
    {
        if (PauseManager.IsPaused) return;

        int count = _active.Count;
        if (count == 0) return;

        int step = Mathf.Min(_maxUpdatePerFrame, count);
        for(int i = 0; i < step; i++)
        {
            if (_cursor >= _active.Count) _cursor = 0;
            var ball = _active[_cursor];
            if(ball == null || !ball.Active)
            {
                _active.RemoveAt(_cursor);
                continue;
            }
            ProcessExpBall(ball, _cursor);
            _cursor++;
        }

    }

    private void ProcessExpBall(ExpBallScript ball, int index)
    {
        Vector3 playerPos = _player.position;
        Vector3 ballPos = ball.transform.position;
        
        Vector3 dist = playerPos - ballPos;

        float sqr = dist.sqrMagnitude;
        float pickupSqr = _pickupRadius * _pickupRadius;

        if (sqr <= pickupSqr)
        {
            Collect(ball, index);
            return;
        }

        float magnetSqr = _magnetRadius * _magnetRadius;
        if (sqr <= magnetSqr)
        {
            float t = 1f - Mathf.Sqrt(sqr) / _magnetRadius;
            float speed = _magnetSpeed * Mathf.Lerp(0.6f, 1.4f, t);

            ball.transform.position = ballPos + dist.normalized * speed * Time.deltaTime;

        }
    }


    private void Collect(ExpBallScript ball, int index)
    {
        _gameManager.PoolManager.Release(ball.gameObject);
        _active.RemoveAt(index);
        _cursor = Mathf.Max(0, _cursor - 1);
        int gained = _gameManager.PlayerExpManager.AddExp(ball.Value);
        _gameManager.GameStats.AddExp(gained);
    }

}