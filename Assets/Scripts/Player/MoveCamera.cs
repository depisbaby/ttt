using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;
    //
    void Update()
    {
        if (cameraPosition == null) return;

        transform.position = cameraPosition.position;
    }
}
