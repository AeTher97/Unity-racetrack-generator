using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class CameraControls : MonoBehaviour {

    private float moveSpeed = 0.1f;
    private float scrollSpeed = 0.5f;
    private float rotationSpeed = 5.0f;
    
    void Update () {
        float translation = Input.GetAxis("Vertical") * moveSpeed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
        float scroll = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;


        transform.Translate(0, scroll, translation);

        transform.Rotate(0, rotation, 0);
    }

}