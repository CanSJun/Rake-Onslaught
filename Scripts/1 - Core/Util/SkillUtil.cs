
using System;
using System.Collections.Generic;
using UnityEngine;

public static  class SkillUtil 
{
    private static readonly Collider[] _target = new Collider[16];

    public static Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {

        t = Mathf.Clamp01(t); // 만약을 위해서

        // 이전까진 Mathf.Pow를 사용했는데 지수/로그 연산을 수행하니 CPU 부하 생김
        // 단순 곱셈으로 전환
        float u = 1f - t; // 1f-t 공통
        float t2 = t * t; // t^2
        float u2 = u * u; // u^2
        float u3 = u2 * u; // u^3
        float t3 = t2 * t; // t^3

        // 베지어 곡선의 공식 B(t)  =  (1 - t)^3 P0 + 3(1-t)^2tP1 + 3(1-t)t^2 P2 + t^3P3

        //     (1-t)^3*p0    3 * (1-t)^2 * p1   3 * (1-t) * t^2 * p2   t^3 * p3  <- 공식 그대로 적용
        return (u3 * p0) + (3f * u2 * t * p1) + (3f * u * t2 * p2) + (t3 * p3);
    }

    public static MissileScript MissileSpawn(Vector3 pos)
    {
        var missile = GameManager.Instance.PoolManager.Spawn(PoolKeys.Missile);
        missile.transform.position = pos;
        return missile.GetComponent<MissileScript>();
    }



    public static GameObject Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
        {
            Debug.LogError("[SkillUtil.Spawn] Prefab is null");
            return null;
        }

        GameObject obj;
        obj = GameManager.Instance.PoolManager.Spawn(prefab.name);
        Transform transform = obj.transform;
        transform.position = position;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        obj.SetActive(true);
        return obj;
    }


}
