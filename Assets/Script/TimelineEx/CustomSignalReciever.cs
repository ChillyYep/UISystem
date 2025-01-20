using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CustomSignalReciever : MonoBehaviour, INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (context is SignalWithParams signalWithParams)
        {
            if (signalWithParams.c == "test")
            {
                Debug.LogError("No Test!");
            }
        }
    }
}
