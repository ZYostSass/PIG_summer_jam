using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public bool isMoving;
    private float footstepTimer = 0.0f;
    public float footstepSpeed = 0.6f;

    [HideInInspector] public float walkSpeed;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        PlayFootsteps();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
        
        // z axis check, for updating music system accordingly
        var areaParam = GetComponent<FMODUnity.StudioEventEmitter>();
        areaParam.SetParameter("Area", transform.position.z + 250.0f);


    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (moveDirection.x != 0 || moveDirection.z != 0)
            isMoving = true;
        else
            isMoving = false;

        // on ground
        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void PlayFootsteps()
    {
        
        if (isMoving)
        {
            // Debug.Log("Checking if able to play footstep. " + footstepTimer);
            if (footstepTimer >= footstepSpeed)
            {
                Debug.Log("Playing footstep.");
                FMODUnity.RuntimeManager.PlayOneShot("event:/Footsteps");
                footstepTimer = 0.0f;
            }
        }
        // Debug.Log(Time.deltaTime);
        footstepTimer = footstepTimer + Time.deltaTime;
        // Debug.Log("footstepTimer : " + footstepTimer);
    }
}
