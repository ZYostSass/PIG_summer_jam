using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody rb;

    [Header("Movement")]
    public float speed = 5f;
    public bool isRunning;
    public Transform orientation;
    Vector3 moveDir;
    Vector3 moveAmount;
    Vector3 smoothMoveVel;

    [Header("Jumping")]
    public float jumpForce = 10f;
    public float raycastDistanceJump = 0.6f;
    private bool isGrounded;
    public float groundDrag;
    private bool canDoubleJump;
    public LayerMask ground;
    
    [Header("Parrying")]
    private bool canParry;
    public float raycastDistanceParry = 1f;
    private Transform parryTarget;
    public LayerMask weapon;

    [Header("Pickup")]
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform pickupTarget;
    [SerializeField] private float pickupRange;
    private Rigidbody currentObject;

    [Header("Dialogue")]
    public bool inDialogue = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //rb.velocity = new Vector3(xSpeed * speed, rb.velocity.y, zSpeed * speed);
        
        // Stop movement if in dialogue
        if (inDialogue) {
            speed = 0;
        }
        else {
            speed = 5f;
        }
        

        // This is for if we want to implement jumping.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistanceJump, ground)) 
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if (isGrounded) 
        {
            rb.drag = groundDrag;
        }
        else 
        {
            rb.drag = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                canDoubleJump = true;
            }
            else if (canDoubleJump) 
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                canDoubleJump = false;
            }
        }

        // This is for a parrying weapon mechanic.
        if (Input.GetMouseButtonDown(0)) 
        {
            ParryAttack();
        }

        // For sprinting.
        if (Input.GetKey(KeyCode.LeftShift)) 
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        ShiftToRun();

        // This is for an item pickup mechanic.
        if (Input.GetMouseButton(1)) 
        {
            if (currentObject)
            {
                currentObject.useGravity = true;
                currentObject = null;
                return;
            }

            Ray cameraRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(cameraRay, out RaycastHit hitInfo, pickupRange, pickupMask))
            {
                currentObject = hitInfo.rigidbody;
                currentObject.useGravity = false;
                Debug.Log(currentObject.name);
            }
        }
    }

    private void FixedUpdate() 
    {
        float xSpeed = Input.GetAxis("Horizontal");
        float zSpeed = Input.GetAxis("Vertical");
        
        moveDir = orientation.forward * zSpeed + orientation.right * xSpeed;
        Vector3 targetMove = moveDir * speed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMove, ref smoothMoveVel, .15f);

        //rb.AddForce(moveDir.normalized * speed, ForceMode.Force);
        rb.MovePosition (rb.position + transform.TransformDirection (moveAmount) * Time.fixedDeltaTime);

        // This is for an item pickup mechanic.
        if (currentObject)
        {
            Vector3 directionToPoint = pickupTarget.position - currentObject.position;
            float distanceToPoint = directionToPoint.magnitude;

            currentObject.velocity = directionToPoint * 12f * distanceToPoint;
        }
    }

    // This is for a parrying weapon mechanic.
    void ParryAttack() 
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.forward * raycastDistanceParry, out hit, raycastDistanceParry, weapon))
        {
            parryTarget = hit.transform;
            parryTarget.GetComponent<Rigidbody>().AddForce(Vector3.left, ForceMode.Impulse);
            Debug.Log(parryTarget.name);
        }
    }

    // For sprinting.
    void ShiftToRun()
    {
        if (!inDialogue) 
        {
            if (isRunning)
            {
                speed = 15f;
            }
            else
            {
                speed = 10f;
            }
        }
    }

    public void onDialogue() 
    {
        inDialogue = true;
    }

    public void exitDialogue()
    {
        inDialogue = false;
    }

    // For debugging purposes.
    private void OnDrawGizmos() {
        Gizmos.DrawRay(transform.position, Vector3.forward * raycastDistanceParry);
    }
}
