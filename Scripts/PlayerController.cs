using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public CharacterController charC;

    public float speed = 5;
    public float gravity = -9.8f;
    public float jumpHeight = 2f;

    Vector3 velocity;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;

  


    // Start is called before the first frame update
    void Start()
    {
        charC.detectCollisions = true;
    }

    // Update is called once per frame
    void Update()
    {
        float xmove = Input.GetAxis("Horizontal");
        float ymove= Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(xmove, ymove);

        //float horizontal = Input.GetAxis("Horizontal");
        // vertical = Input.GetAxis("Vertical");

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        Vector3 moveHor = transform.right * movement.x + transform.forward * movement.y;
        charC.Move(moveHor*speed*Time.deltaTime);

        velocity.y += gravity * Time.deltaTime * 2; //Create downward force to simulate gravity becasue we are not using a rigidbody
        charC.Move(velocity* Time.deltaTime); 

        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }


        if(isGrounded && velocity.y <0) //if the player is grounded and with velocity less than 0
        {
            velocity.y = -2f; //reset velocity
        }
    }
}
