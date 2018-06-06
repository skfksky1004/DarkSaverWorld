using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    protected static T instance = null;
    public static T Instance
    {
        get
        {
			instance = FindObjectOfType<T>() as T;

            if (instance == null)
            {
                System.Type type = typeof(T);

                if(instance == null)
                {
                    string typeName = type.ToString();

                    GameObject gameObject = new GameObject(typeName, type);
                    instance = gameObject.GetComponent<T>();

                    if(instance == null)
                    {
                        Debug.Log("not found Instance");
                    }
                }
            }
            else
            {
                Initialize(instance);
            }

            return instance;
        }
    }
    
    static void Initialize(T instance)
    {
        if (MonoSingleton<T>.instance == null)
        {
            MonoSingleton<T>.instance = instance;
        }
        else if (MonoSingleton<T>.instance != instance)
        {
            DestroyImmediate(instance.gameObject);
        }
    }

    static void Destroyed(T instance)
    {
        if (MonoSingleton<T>.instance == instance)
        {
            MonoSingleton<T>.instance = null;
        }
    }

    protected abstract void OnInit();
    protected abstract void OnExit();

    private void Awake()
    {
        OnInit();

        Initialize(this as T);
    }

    private void OnApplicationQuit()
    {
        OnExit();

        Destroyed(this as T);
    }
}
