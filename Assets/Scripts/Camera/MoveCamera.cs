using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform CameraPostion;

    void Update()
    {
        transform.position = CameraPostion.position;
    } 
}
