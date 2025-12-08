using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                // 尝试在场景中查找
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    Debug.LogWarning($"[Singleton<{typeof(T).Name}>] No instance found in scene.");
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != (T)this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = (T)this;
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}

