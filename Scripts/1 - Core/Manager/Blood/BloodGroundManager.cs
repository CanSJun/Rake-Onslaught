using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodGroundManager
{

    private int _maxCount = 30;

    private readonly Queue<GameObject> _bloodQueue = new();


    public void AddBlod(GameObject blood)
    {
        _bloodQueue.Enqueue(blood);

        if(_bloodQueue.Count > _maxCount)
        {
            GameObject oldBlood = _bloodQueue.Dequeue();
            GameManager.Instance.PoolManager.Release(oldBlood);
        }
    }

}