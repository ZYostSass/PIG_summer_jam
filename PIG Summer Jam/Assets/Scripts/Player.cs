using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody rb;

    [Header("Movement")]
    public float speed = 10f;
    public bool isRunning;
    
    public Transform orientation;
    Vector3 moveDir;

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

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //rb.velocity = new Vector3(xSpeed * speed, rb.velocity.y, zSpeed * speed);

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

        if (Input.GetMouseButtonDown(0)) 
        {
            ParryAttack();
        }

        if (Input.GetKey(KeyCode.LeftShift)) 
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        ShiftToRun();

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
        
        rb.AddForce(moveDir.normalized * speed, ForceMode.Force);

        if (currentObject)
        {
            Vector3 directionToPoint = pickupTarget.position - currentObject.position;
            float distanceToPoint = directionToPoint.magnitude;

            currentObject.velocity = directionToPoint * 12f * distanceToPoint;
        }
    }

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

    void ShiftToRun()
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

    private void OnDrawGizmos() {
        Gizmos.DrawRay(transform.position, Vector3.forward * raycastDistanceParry);
    }
}
