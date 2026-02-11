using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;


[AttributeUsage(AttributeTargets.Field)]
public class GetComponentsAttribute : Attribute
{
    public string[] _attributeName { get; }
    public GetComponentsAttribute(params string[] attributeName)
    {
        _attributeName = attributeName;
    }

}

public static class Util
{
    private static int _start;
    private static CameraScript _cameraScript;


    public static void OnStart() => _start = 1;
    public static int START => _start;

    /// <summary>
    /// 내 자식에서 Name이란 컴포넌트 가지고 있는 자식 찾기
    /// </summary>
    /// <param name="name"> 찾을 컴포넌트 이름</param>
    /// <param name="root"></param>
    /// <returns></returns>
    public static Transform FindingChild(string name, Transform root)
    {
        if (root.name == name) return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindingChild(name, root.GetChild(i));
            if (found != null) return found;
        }
        return null;
    }

/// <summary>
/// 찾은 컴포넌트 적용
/// </summary>
/// <param name="obj"></param>
    public static void ApplyComponents(object obj)
    {
        if (obj is not MonoBehaviour mono)
        {
            Debug.LogError("ApplyComponents 는 MonoBehaviour 만 지원합니다.");
            return;
        }

        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<GetComponentsAttribute>();
            if (attribute == null) continue;

            Type fieldType = field.FieldType;

            if (fieldType.IsArray)
            {
                Type elementType = fieldType.GetElementType();
                List<Component> list = new();

                foreach (string childName in attribute._attributeName)
                {
                    Transform t = FindingChild(childName, mono.transform);
                    if (t == null)
                    {
                        Debug.LogWarning($"[{childName}] 자식 Transform 을 찾지 못했습니다.");
                        continue;
                    }

                    Component comp = t.GetComponent(elementType);
                    if (comp == null)
                    {
                        Debug.LogWarning($"[{t.name}] 오브젝트에 {elementType.Name} 컴포넌트가 없습니다.");
                        continue;
                    }
                    list.Add(comp);
                }

                Array arr = Array.CreateInstance(elementType, list.Count);
                for (int i = 0; i < list.Count; i++) arr.SetValue(list[i], i);
                field.SetValue(mono, arr);
            }
            else
            {
                string targetName = attribute._attributeName[0];
                Transform t = FindingChild(targetName, mono.transform);
                if (t == null)
                {
                    Debug.LogWarning($"[{targetName}] 자식 Transform 을 찾지 못했습니다.");
                    continue;
                }
                Component comp = t.GetComponent(fieldType);
                if (comp == null)
                {
                    Debug.LogWarning($"[{t.name}] 오브젝트에 {fieldType.Name} 컴포넌트가 없습니다.");
                    continue;
                }
                field.SetValue(mono, comp);
            }
        }
    }

    /// <summary>
    /// 총알을 생성하는 함수
    /// </summary>
    /// <param name="poolName"> Object Pooling Name</param>
    /// <param name="prefab"> Bullet Prefab </param>
    /// <param name="dir"> Direction </param>
    /// <param name="startPos"> Muzzle Position </param>
    /// <param name="damage"> Damage </param>
    /// <param name="speed"> Bullet Speed </param>
    /// <param name="lifeTime"> Bullet LifeTime </param>
    /// <param name="c"> Bullet Color </param>
    /// <param name="radius"> Bullet radius </param>
    /// <returns></returns>
    public static void Shoot(string weaponId, string poolName, GameObject prefab, Vector3 dir, Vector3 startPos,float damage, float speed, float lifeTime, Color c, float radius)=> GameManager.Instance.BulletManager.Fire(startPos, dir, speed, damage, lifeTime, radius, c, poolName, weaponId);
    /// <summary>
    /// 총알 퍼지게 적용하는 함수
    /// </summary>
    /// <param name="dir"> Muzzle Direction </param>
    /// <param name="angle"> Spread </param>
    /// <returns></returns>
    public static Vector3 ApplySpread(Vector3 dir, float angle)
    {
        float yaw = UnityEngine.Random.Range(-angle, angle);
        float pitch = UnityEngine.Random.Range(-angle, angle);
        return Quaternion.Euler(pitch, yaw, 0) * dir;
    }

    public static void TextChange(Text text, string str) => text.text = str;


    public static void CameraScriptSetting(CameraScript camera) => _cameraScript = camera;

    public static void ShakeCamera(float duration, float power)
    {
        if (_cameraScript == null) { return; }
        _cameraScript.SetShake(duration, power);
    }
    

    public static Vector3 RandomCircle(Vector3 center, float maxR, float minR)
    {
        float r = UnityEngine.Random.Range(minR, maxR);
        float ang = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;

        return center + new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);

    }


    /// <summary>
    /// XZ 평면상에서 선분과 원의 충돌을 감지합니다.
    /// </summary>
    /// <param name="p1">선분 시작점</param>
    /// <param name="p2">선분 끝점</param>
    /// <param name="circleCenter">원 중심</param>
    /// <param name="radius">원 반지름</param>
    /// <returns>충돌하면 true, 아니면 false, 어디에 맞았는지 알 수 있게 t로 out해놨음</returns>
    public static bool IsLineSegmentIntersectingCircleXZ(Vector3 p1, Vector3 p2, Vector3 circleCenter, float radius, out float t)
    {

        /*
        ㅁ선분 벡터 AB = P2 - P1
        ㅁ시작점 ~ 원중심 벡터 AC = C - P1
        ㅁProjection을 통해 t 계산 
        원 중심 C를 선분이 포함된 직선에 수직으로 내렸을 때의 위치비율 t를 구하는 공식
        t = (AC * AB) / |AB|^2  (내적을 선분 길이의 제곱으로 나눈 값)

       ㅁ범위 제한
        -선분은 길이가 유한하므로 t의 값을 0과 1 사이로 제한!

        t < 0 이면 t = 0 즉 시작점 P1이 가장 가까움
        t > 1 이면 t = 1 끝쩜 p2가 가장 가까움 
        그 외에는 계산된 t 그대로 사용

        ㅁ가장 가까운 점을 계산
        Pclosest = P1 + (AB * t)

        ㅁ충돌 판정
        원 중심 C와 Pcloses 사이의 거리를 구한다
        d^2 <= r^2이면 충돌! (제곱근 연산을 피하기 위해 거리의 제곱을 비교!)
        */

        //== 선분 벡터 ==
        float dx = p2.x - p1.x;
        float dz = p2.z - p1.z;

        // == 제곱 == 
        float lenSq = (dx * dx) + (dz * dz);  // 0으로 나누는 것을 방지

        // == 선분의 길이가 거의 0 즉 점이라면, 점과 원으로 충돌 처리 
        if(lenSq < 1e-6f)
        {
            t = 0f;
            float distance = ((p1.x - circleCenter.x) * (p1.x - circleCenter.x)) + ((p1.z - circleCenter.z) * (p1.z - circleCenter.z));
            return distance <= (radius * radius);
        }

        // == 투영 비율 t 계산 ==   t = (AC * AB) / |AB|^2 
        // t = (AC dot AB) / lenSq

        float ac_x = circleCenter.x - p1.x;
        float ac_z = circleCenter.z - p1.z;

        float dotProduct =(ac_x * dx) + (ac_z * dz);
        t = dotProduct / lenSq;


        // t를 0 ~ 1 구간으로 Clamp -> 왜냐하면 선분 밖으로 나가지 않도록 제한하기 위해서
        if (t < 0.0f) t = 0.0f;
        else if (t > 1.0f) t = 1.0f;

        // == 선분 위의 가장 가까운 점 계산  P1 + (AB * t)

        float closestX = p1.x + dx * t;
        float closestZ = p1.z + dz * t;

        // == 원 중심과 가장 가까운 점 사이의 거리 제곱 계산

        float distX = circleCenter.x - closestX;
        float distZ = circleCenter.z - closestZ;

        float distanceSq = (distX * distX) + (distZ * distZ);

        // 반지름 제곱과 비교해서 제곱근 연산 최적화

        return distanceSq <= (radius * radius);


    }


    public static bool IsSweptSegmentIntersectingMovingCircleXZ(Vector3 bulletPrev, Vector3 bulletCurr, Vector3 monsterPrev, Vector3 monsterCurr, float combinedRadius, out float t)
    {
        // y는 무시
        bulletPrev.y = bulletCurr.y = 0f;
        monsterPrev.y = monsterCurr.y = 0f;
        // 상대공간으로 변환을 해서  몬스터를 정지시킨 좌표계에서 총알이 움직이는 것처럼 만들어줍니다.
        Vector3 rel0 = bulletPrev - monsterPrev;
        Vector3 rel1 = bulletCurr - monsterCurr;
        // 원 중심은 원점
        return IsLineSegmentIntersectingCircleXZ(rel0, rel1, Vector3.zero, combinedRadius, out t);
    }

    public static void ResetStart() => _start = 0;
}
