using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class PlayerMovement : NetworkBehaviour
{
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _cam = Camera.main;
    }

    private void Update()
    {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    #region Movement

    [SerializeField] private float _acceleration = 80;
    [SerializeField] private float _maxVelocity = 10;
    private Vector3 _input;
    private Rigidbody _rb;

    private void HandleMovement()
    {
        _rb.velocity += _input.normalized * (_acceleration * Time.deltaTime);
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, _maxVelocity);
    }

    #endregion

    #region Rotation

    [SerializeField] private float _rotationSpeed = 450;
    private Plane _groundPlane = new(Vector3.up, Vector3.zero);
    private Camera _cam;

    private void HandleRotation()
    {
        var ray = _cam.ScreenPointToRay(Input.mousePosition);

        if (_groundPlane.Raycast(ray, out var enter))
        {
            var hitPoint = ray.GetPoint(enter);

            var dir = hitPoint - transform.position;
            var rot = Quaternion.LookRotation(dir);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _rotationSpeed * Time.deltaTime);
        }
    }

    #endregion

    //public Camera cam;
    //[SerializeField]
    //private float mouseSens = 300f;
    //[SerializeField]
    //private Transform playerBody;
    //private float xRotation = 0f;

    //[SerializeField]
    //private CharacterController controller;

    //public float PSpeed;
    //public float PJump = 3f;
    //public float gravity = -22.81f;
    //public Vector3 vel;

    //public Transform groundCheck;
    //public float groundDist = 0.4f;
    //public LayerMask groundMask;

    //bool isGrounded;

    //public override void OnNetworkSpawn()
    //{
    //    if (!IsOwner) Destroy(this);
    //}


    //// Update is called once per frame
    //void Update()
    //{
    //    //camera stuff
    //    float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
    //    float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

    //    xRotation -= mouseY;
    //    xRotation = Mathf.Clamp(xRotation, -10f, 10f);

    //    cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

    //    playerBody.Rotate(Vector3.up * mouseX);
    //    //camera stuff


    //    isGrounded = Physics.CheckSphere(groundCheck.position, groundDist, groundMask);

    //    if (isGrounded && vel.y < 0)
    //    {
    //        vel.y = -2f;
    //    }

    //    float x = Input.GetAxis("Horizontal");
    //    float z = Input.GetAxis("Vertical");

    //    Vector3 movement = transform.right * x + transform.forward * z;

    //    controller.Move(movement * PSpeed * Time.deltaTime);

    //    if (Input.GetButtonDown("Jump") && isGrounded)
    //    {
    //        vel.y = Mathf.Sqrt(PJump * -2f * gravity);
    //    }

    //    vel.y += gravity * Time.deltaTime;

    //    controller.Move(vel * Time.deltaTime);
    //}
}
