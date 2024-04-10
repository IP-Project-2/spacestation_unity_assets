using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerlook : MonoBehaviour {
    // reference our player
    public Transform playerBody;

    // choose how sensitive the mouse will be
    public float mouseSensitivity = 100f;

    // Use this to pass the Yrotation in transform
    float xRotation = 0f;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked; //locks our cursor in the middle of the screen
    }

    // Update is called once per frame
    void Update() {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        playerBody.Rotate(Vector3.up * mouseX); //rotate player body with mouse
        xRotation -= mouseY; //passsing our mouse Y rotation
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); //clamp the rotation to specific angles


        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); //rotate the camera
    }
}
