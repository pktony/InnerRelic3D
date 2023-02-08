using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 싱글톤 제네릭 클래스
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance = null;

    public bool isDontDestroy = false;

    /// <summary>
    /// 인스턴스가 없으면 먼저 찾아보고
    /// 그래도 없으면 새로운 게임오브젝트 생성 
    /// </summary>
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
                }
                else
                { // 없으면 생성 
                    GameObject gameObject = new GameObject();
                    gameObject.name = $"{typeof(T).Name}";
                    instance = gameObject.AddComponent<T>();
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
            if (isDontDestroy)  // True 일때만 DontDestroy
                DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if (instance != this)
            {
                Destroy(this.gameObject);
            }
        }
    }
}