using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class Singleton_Unity<T> : MonoBehaviour where T : class
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            Assert.IsNotNull(_instance);
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        _instance = this as T;
    }
}
