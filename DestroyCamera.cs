using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCamera : MonoBehaviour
{
    void Awake()
    {
        GameObject[] cameraObject = GameObject.FindGameObjectsWithTag("FollowCamera");
        Debug.Log(cameraObject.Length);
        if (cameraObject.Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
