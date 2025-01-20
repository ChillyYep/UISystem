using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalReact : MonoBehaviour
{
    public void Dosomething(string signalName)
    {
        switch(signalName)
        {
            case "Hit":
                Debug.Log("Hit Someone!");
                break;
        }
    }
}
