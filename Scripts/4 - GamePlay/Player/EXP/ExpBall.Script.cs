using UnityEngine;

public class ExpBallScript : MonoBehaviour
{
    public int Value { get; private set; }
    internal bool Active { get; private set; }

    public void Activate(int value, Vector3 pos)
    {
        Value = value;
        transform.position = pos;
        Active = true;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        Active = false;
        gameObject.SetActive(false);
    }
}
