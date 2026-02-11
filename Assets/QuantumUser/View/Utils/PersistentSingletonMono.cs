using UnityEngine;

public abstract class PersistentSingletonMono<T> : MonoBehaviour
    where T : PersistentSingletonMono<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = (T)this;
        DontDestroyOnLoad(gameObject);
    }
}
