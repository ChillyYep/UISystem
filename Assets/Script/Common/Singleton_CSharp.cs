using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton_CSharp<T> where T : new()
{
    protected Singleton_CSharp() { }
    private static T _instance;
    public static T Instance => _instance == null ? (_instance = new T()) : _instance;
}
