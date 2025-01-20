using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Action act = () => Debug.Log("Act1");

        act += () => Debug.Log("Act2");
        act += () => Debug.Log("Act3");
        //act = () => Debug.Log("Act4");
        //act();

        foreach(var delegateF in act.GetInvocationList())
        {
            delegateF.DynamicInvoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
