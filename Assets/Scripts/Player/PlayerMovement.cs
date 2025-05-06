using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // global variables
    public Rigidbody rb;
    public Transform orientation;

    // Movement settings
    public float moveSpeed;
    public float jumpHeight;
    public float airControl;
    public float slopeControl;
    public float groundDrag;
    public float airDrag;
    public float slopeDrag;

    private Vector3 moveDirection;
    [SerializeField] private bool isGrounded; // Tracks if the player is on the ground (not including slopes)
    [SerializeField] private bool isJumping; // Tracks if the player is jumping
    private float jumpCooldown = 0.1f;
    RaycastHit slopeHit; // Raycast hit information for slope detection
    public float maxSlope;

    void Start()
    {
        rb.freezeRotation = true; // Prevents the player from tipping over
        rb.useGravity = false; // Disable default gravity
                
        // set the initial speed and drag values
        rb.drag = groundDrag;
        
        // set the initial position, rotation, and scale of the player
        transform.position = new Vector3(0, 2, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = new Vector3(1, 1, 1);
    }

    void Update()
    {
        GetInputs();
    }

    void GetInputs(){
        
        // Collect keyboard input
        Vector3 playerInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (playerInput.magnitude > 1)
            playerInput.Normalize();

        // Assign the input to the move direction
        moveDirection = orientation.forward * playerInput.z + orientation.right * playerInput.x;

        // Assign the input to the jumping satus
        isJumping = Input.GetKey(KeyCode.Space) ? true:false;
    }

    void FixedUpdate()
    {
        MovePlayer();
        JumpPlayer();
        Player_Debug();
    }

    void MovePlayer()
    {

        if (IsOnSlope())
        {
            //Debug.Log("On slope");

            // Set grounded status to true when on slope
            isGrounded = true;
            
            // Get the move direction reletive to the angle of the slope
            Vector3 slopeDir = GetSlopeMoveDirection();

            // Gradually redirect velocity while on slope
            rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity.magnitude * slopeDir, Time.fixedDeltaTime * slopeControl);

            // Apply movement with slope control scaler (<1) 
            rb.drag = slopeDrag; // Set drag for slope
            rb.AddForce(slopeDir* moveSpeed * slopeControl, ForceMode.Force);
        }
        else if (isGrounded)
        {
            //Debug.Log("On ground");
            
            // Apply movement
            rb.drag = groundDrag; // Set drag for ground
            rb.AddForce(moveDirection * moveSpeed, ForceMode.Force);
        }
        else
        {
            //Debug.Log("In air");
            
            // Gradually redirect velocity while on slope
            rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity.magnitude * orientation.forward, Time.fixedDeltaTime * airControl);
            
            // Apply movement with air control scaler (<0.5)
            rb.drag = airDrag; // Set drag for air
            rb.AddForce(moveDirection * moveSpeed * airControl, ForceMode.Force);

            // Apply artificial gravity
            ApplyGravity(Vector3.down, 1);
        }
    }

    void JumpPlayer()
    {   
        jumpCooldown -= Time.deltaTime; // Decrease jump cooldown
        
        if (isJumping && isGrounded && jumpCooldown <= 0)
        {
            Debug.Log("Jump!");
            
            // Apply jump force
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            //rb.velocity += new Vector3(0,jumpHeight,0);

            // Reset cooldown
            jumpCooldown = 0.1f;
        }
    }
    private bool IsOnSlope()
    {
        // Raycast slightly down to detect if standing on a slanted surface
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 1.3f))
        {
            // Draw the ray in the Scene view (green if it hits, red otherwise)
            Debug.DrawRay(transform.position, Vector3.down * 1.3f, Color.green);

            float slopeAngle = Vector3.Angle(slopeHit.normal, Vector3.up);
            isGrounded = slopeAngle < 5f; // Consider nearly flat ground as "grounded"
            return maxSlope > slopeAngle && slopeAngle > 5f && slopeAngle < 85f; // Surf slope if tilted
        }

        // Draw the ray in red if it doesn't hit anything
        Debug.DrawRay(transform.position, Vector3.down * 1.3f, Color.red);

        isGrounded = false;
        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        // Calculate the slope move direction
        Vector3 slopeDirection = Vector3.ProjectOnPlane(orientation.forward, slopeHit.normal).normalized;

        // Draw the ray in the Scene view to visualize the slope direction
        Debug.DrawRay(transform.position, slopeDirection * 10f, Color.blue);

        return slopeDirection;
    }

    /// <summary>
    /// Applies gravity to the player. Based on a normilized direction and a scale.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="scale"></param>
    private void ApplyGravity(Vector3 direction, float scale)
    {
        // Apply gravity to the player
        rb.AddForce(direction * 9.81f * scale, ForceMode.Acceleration);
    }

    // DEBUGGING - - - - - - - - - - - - - - - - -
    void Player_Debug(){
        if (Input.GetKey(KeyCode.Tab)){
            transform.parent.transform.position = new Vector3(0, 2, 0);
            rb.velocity = Vector3.zero;
        }

        //Debug.Log("Player Inputs: " + playerInput);
    }
}
