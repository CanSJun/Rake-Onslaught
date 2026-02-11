using UnityEngine;
public class PoolIdentity : MonoBehaviour
{
    [SerializeField] private string poolKey;
    public string Id => poolKey;
}