
using UnityEngine;
public class SingletonManager<T> where T : new()
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = new T();

            return instance;
        }
    }
}
public class MonoSingletonManager<T> : MonoBehaviour where T : MonoSingletonManager<T>
{
    public static T Instance
    { get; protected set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
}
