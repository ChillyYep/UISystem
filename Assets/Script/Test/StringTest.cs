using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string str1 = "Hello!";
        string str2 = "Hello!";
        string str3 = "Hell";
        str3 += "o!1";
        Debug.Log($"StringTest0 :{string.IsInterned(str3)}");
        string str4 = str3;
        str3 = string.Intern(str3);
        Debug.Log($"StringTest1 :{ReferenceEquals(str1, str2)}");
        Debug.Log($"StringTest2 :{ReferenceEquals(str1, str3)}");
        Debug.Log($"StringTest3 :{ReferenceEquals(str3, str4)}");

    }

    // Update is called once per frame
    void Update()
    {

    }
}
