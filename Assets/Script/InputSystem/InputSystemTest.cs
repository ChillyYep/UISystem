using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemTest : MonoBehaviour
{
    public Transform target;

    public Transform virtualCamera;

    public Transform other;

    public float speed = 1f;

    // Update is called once per frame
    void Update()
    {
        float characterHorizontal = Input.GetAxis(XBoxInputCode.LeftJoyStickHorizontal);
        float characterVertical = Input.GetAxis(XBoxInputCode.LeftJoyStickVertical);

        float cameraHorizontal = Input.GetAxis(XBoxInputCode.RightJoyStickHorizontal);
        float cameraVertical = Input.GetAxis(XBoxInputCode.RightJoyStickVertical);

        float lt = Input.GetAxis(XBoxInputCode.LT);
        float rt = Input.GetAxis(XBoxInputCode.RT);

        if (target != null)
        {
            target.transform.localPosition += new Vector3(characterHorizontal, 0, characterVertical) * speed;
        }
        if (virtualCamera != null)
        {
            virtualCamera.transform.localPosition += new Vector3(cameraHorizontal, 0, cameraVertical) * speed;
        }
        if (other != null)
        {
            other.transform.localPosition += new Vector3(lt, 0, rt) * speed;
        }
        for (KeyCode keyCode = KeyCode.Joystick1Button0; keyCode < KeyCode.Joystick1Button19; ++keyCode)
        {
            if (Input.GetKey(keyCode))
            {
                Debug.Log($"{keyCode} pressed!");
            }
        }

    }
}
