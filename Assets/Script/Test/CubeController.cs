using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    public float speed = 1f;
    public Transform cameraTrans;

    private void Update()
    {
        var oldPos = transform.position;
        if (Input.GetKeyDown(KeyCode.W))
        {
            transform.position += Vector3.forward * speed;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            transform.position += Vector3.right * speed;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            transform.position -= Vector3.forward * speed;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            transform.position -= Vector3.right * speed;
        }
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out var hitInfo, 4f, LayerMask.GetMask("Ground")))
        {
            Debug.Log($"Raycast HitInfo {hitInfo}");
            transform.position = new Vector3(transform.position.x, hitInfo.point.y + 2f, transform.position.z);
        }
        if (cameraTrans != null)
        {
            cameraTrans.transform.position += transform.position - oldPos;
        }
    }
}
