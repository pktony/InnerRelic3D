using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance = null;

    public bool isDontDestroy = false;

    public static T Inst
    {
        get
        {
            if (instance == null)
            {
                T obj = FindObjectOfType<T>();
                if (obj != null)
                {
                    instance = obj;
                    Debug.Log("Old Instance found");
                }
                else
                { // 없으면 생성 
                    GameObject gameObject = new GameObject();
                    gameObject.name = $"{typeof(T).Name}";
                    instance = gameObject.AddComponent<T>();
                    Debug.Log("New Instance Created");
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            T obj = this as T;
            if (isDontDestroy)
                DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if (instance != this)
            {
                Destroy(this.gameObject);
                Debug.Log("New instance Destroyed");
            }
        }
    }
}