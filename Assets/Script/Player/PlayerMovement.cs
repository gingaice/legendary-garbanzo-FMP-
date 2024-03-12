using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Camera cam;
    [SerializeField]
    private float mouseSens = 300f;
    [SerializeField]
    private Transform playerBody;
    private float xRotation = 0f;

    [SerializeField]
    private CharacterController controller;

    public float PSpeed;
    public float PJump = 3f;
    public float gravity = -22.81f;
    public Vector3 vel;

    public Transform groundCheck;
    public float groundDist = 0.4f;
    public LayerMask groundMask;

    bool isGrounded;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //camera stuff
        float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -10f, 10f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
        //camera stuff


        isGrounded = Physics.CheckSphere(groundCheck.position, groundDist, groundMask);

        if (isGrounded && vel.y < 0)
        {
            vel.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 movement = transform.right * x + transform.forward * z;

        controller.Move(movement * PSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            vel.y = Mathf.Sqrt(PJump * -2f * gravity);
        }

        vel.y += gravity * Time.deltaTime;

        controller.Move(vel * Time.deltaTime);
    }
}
