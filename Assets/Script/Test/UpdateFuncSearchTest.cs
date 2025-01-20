using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateFuncSearchTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var methodInfo = GetType().GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        Debug.Log(nameof(UpdateFuncSearchTest) + $" Start:fieldInfo {methodInfo != null}");
    }
    //private void Update()
    //{
        
    //}
}
