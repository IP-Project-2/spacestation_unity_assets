using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerlook : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform playerBody; //reference our player

    public float mouseSensitivity = 100f; //choose how sensitive the mouse will be


    float xRotation = 0f; //Use this to pass the Yrotation in transform
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //locks our cursor in the middle of the screen
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity*Time.deltaTime; //get the mouse X from unity input Time.deltaTime normalizes the time so we wont look faster if the frame rate is high
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity*Time.deltaTime; //get the mouse y from unity input Time.deltaTime normalizes the time so we wont look faster if the frame rate is high
        playerBody.Rotate(Vector3.up * mouseX); //rotate player body with mouse
        xRotation -= mouseY; //passsing our mouse Y rotation
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); //clamp the rotation to specific angles


        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); //rotate the camera
    }
}
