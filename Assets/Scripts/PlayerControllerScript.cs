using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed;     // Walking speed of the player
    public float groundDrag;    // Drag applied when on the ground
    public float airDrag;       // Drag applied when in the air
    public float airMultiplier; // Multiplier for movement speed while in the air
    public float airSpeedCap;
    float speed = 5f;           // Base movement speed
    public MovementState movementState;  // Tracks the player's current movement state

    [Header("Jumping")]
    public float jumpForce;     // Force applied when the player jumps
    public float jumpDelay;     // Delay between consecutive jumps
    bool readyToJump = true;    // Whether the player can jump again

    [Header("Sprinting")]
    public float sprintSpeed;   // Speed during sprinting

    [Header("Crouching")]
    public float crouchSpeed;   // Speed during crouching
    public float startYScale;   // Original Y scale of the player
    public float crouchYScale;  // Y scale while crouching
    public float crouchForce;   // Downward force applied when crouching

    [Header("Slope Movement")]
    public float maxSlopeAngle; // Maximum angle of a slope the player can walk on
    RaycastHit slopeCast;       // Stores information about the slope being detected
    public float slopeCling = 5;  // Smoothing speed for aligning the player to the slope

    [Header("Ground Check")]
    public float playerHeight;  // Height of the player collider
    public LayerMask groundLayer;  // Layer mask used to determine what is considered ground
    bool grounded = false;      // Whether the player is on the ground

    


    [Header("Wallrunning")]
    public LayerMask wallLayer;
    public float wallRunForce;
    public float maxWallTime;
    public float wallRunTimer;
    public float wallJumpVerticalForce;
    public float wallJumpHorizontalForce;

    [Header("Wall Detection")]
    public float wallCheckDist;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;


    [Header("Grappling")]
    public bool isGrappled;
    public float grappleForce;
    public float grappleStretch;
    public float grappleYankPercentDistance;
    public Vector3 currentGrapplePoint;
    LineRenderer lineRenderer;

    [Header("References")]
    //public GameObject player;
    private Rigidbody _rbody;   // Reference to the player's Rigidbody component
    private CapsuleCollider _collider;  // Reference to the player's CapsuleCollider component
    private Vector2 moveVal;    // Stores movement input values
    private InputAction sprintAction;  // Reference to the sprint input action
    private InputAction crouchAction;  // Reference to the crouch input action
    Vector3 moveDirection;      // Calculated movement direction
    public Transform orientation; // Reference to your camera's orientation
    public GameObject cam;
    CameraControllerScript camScript;

    public enum MovementState
    {
        walking,    // Player is walking
        sprinting,  // Player is sprinting
        wallrunning,// Player is wallrunning
        crouching,  // Player is crouching
        inAir       // Player is in the air
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        _collider = GetComponent<CapsuleCollider>(); 
        _rbody = GetComponent<Rigidbody>(); 
        sprintAction = GetComponent<PlayerInput>().actions["Sprint"]; // Get the sprint input action
        crouchAction = GetComponent<PlayerInput>().actions["Crouch"]; // Get the crouch input action
        startYScale = transform.localScale.y; // Store the original Y scale of the player
        camScript = cam.GetComponent<CameraControllerScript>();
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        StateHandler(); // Update the player's movement state
        moveDirection = orientation.right * moveVal.x + orientation.forward * moveVal.y;    // Calculate movement direction based on input

        //Grapple Rendering
        if (isGrappled)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, currentGrapplePoint);
        }
        
    }

    void FixedUpdate()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer); // Ground Check
        Debug.DrawLine(transform.position, transform.position - new Vector3(0, playerHeight * 0.5f + 0.2f, 0), Color.red, 0.01f); // Show ground check ray

        if (grounded)
        {
            //Debug.Log("GROUNDED");
            _rbody.drag = groundDrag;   // Apply ground drag
        }
        else
        {
            _rbody.drag = airDrag;      // Apply air drag
        }

        HandleMovement(); 
        WallCheck();
    }

    private void StateHandler()
    {
        // First, handle scale changes for crouching
        if (crouchAction.IsPressed() && grounded)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (movementState != MovementState.crouching)
            {
                _rbody.AddForce(new Vector3(0, -crouchForce, 0), ForceMode.Impulse);
            }
            movementState = MovementState.crouching;
            speed = crouchSpeed;
        }
        else
        {
            // Reset scale when not crouching
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

            // Then handle other states
            if ((wallLeft || wallRight) && moveVal.y > 0.1f && sprintAction.IsPressed() && AboveGround())
            {
                if (movementState != MovementState.wallrunning)
                {
                    StartWallrunning();
                    speed = sprintSpeed;
                }
            }
            // Then handle other states
            else if (grounded && sprintAction.IsPressed())
            {
                if (movementState != MovementState.sprinting)
                {
                    camScript.ResetCameraEffects(false);
                    camScript.SprintZoom();
                    movementState = MovementState.sprinting;
                    speed = sprintSpeed;
                }
            }
            else if (grounded)
            {
                if (movementState != MovementState.walking)
                {
                    camScript.ResetCameraEffects(false);
                    movementState = MovementState.walking;
                    speed = walkSpeed;
                }
            }
            else
            {
                if (movementState != MovementState.walking)
                    camScript.ResetCameraEffects(true);
                movementState = MovementState.inAir;
            }
        }
    }

    void OnMove(InputValue value)
    {
        moveVal = value.Get<Vector2>(); // Get movement input from the player
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed && readyToJump && (grounded || movementState == MovementState.wallrunning))
        {
            PlayerJump();  // Perform jump
            readyToJump = false;  // Prevent immediate consecutive jumps
            Invoke(nameof(ResetJump), jumpDelay);  // Reset jump after a delay
        }
    }

    private void HandleMovement()
    {
        // ON SLOPE
        if (OnSlopeCheck())
        {
            _rbody.AddForce(10f * speed * GetSlopeDirection(), ForceMode.Force); // Move along the slope direction
            _rbody.AddForce(slopeCling * -slopeCast.normal, ForceMode.Force);   // Apply force to cling to slope
            Debug.Log("ON SLOPE");
        }
        // CROUCHING
        else if (movementState == MovementState.crouching)
        {
            _rbody.AddForce(10f * crouchSpeed * moveDirection.normalized, ForceMode.Force); // Apply crouch movement force
        }
        // WALKING OR SPRINTING
        else if (movementState == MovementState.walking || movementState == MovementState.sprinting)
        {
            _rbody.AddForce(10f * speed * moveDirection.normalized, ForceMode.Force); // Apply movement force on the ground
        }
        // WALLRUNNING
        else if (movementState == MovementState.wallrunning)
        {

            _rbody.useGravity = false;
            Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

            if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            {
                wallForward = -wallForward;
            }

            if ((wallLeft && moveVal.x < 0) || (wallRight && moveVal.x > 0)) // Wall cling for convex walls
            {
                _rbody.AddForce(-wallNormal * 100, ForceMode.Force);
            }

            _rbody.AddForce(wallForward * wallRunForce * 100, ForceMode.Force);
     
        }
        // IN AIR
        else if (movementState == MovementState.inAir)
        {
            _rbody.AddForce(10f * speed * airMultiplier * moveDirection.normalized, ForceMode.Force); // Apply movement force in the air
        }

        // SPEED CONTROL
        Vector3 xzVel = new Vector3(_rbody.velocity.x, 0f, _rbody.velocity.z); // Get horizontal velocity

        if (xzVel.magnitude > speed && !isGrappled)
        {
            Vector3 cappedVel = xzVel.normalized * speed; // Limit velocity to max speed
            _rbody.velocity = new Vector3(cappedVel.x, _rbody.velocity.y, cappedVel.z); // Apply capped velocity
        }

        _rbody.useGravity = !OnSlopeCheck(); // Disable gravity on slopes to stop sliding
    }


    private void PlayerJump()
    {
        if (movementState == MovementState.wallrunning)
        {
            Debug.Log("WallJump");
            _rbody.velocity = new Vector3(_rbody.velocity.x, 0, _rbody.velocity.z); // Reset vertical velocity before jumping
            Vector3 wallNormal = wallLeft ? leftWallHit.normal : rightWallHit.normal;

            Vector3 jumpDirection = ((wallJumpVerticalForce * orientation.up) + wallJumpHorizontalForce * wallNormal); // how much vertical v.s. lateral force is there?
            _rbody.AddForce(jumpDirection, ForceMode.Impulse);           // Apply wall jump force

        } else
        {
            _rbody.velocity = new Vector3(_rbody.velocity.x, 0, _rbody.velocity.z); // Reset vertical velocity before jumping
            _rbody.AddForce(orientation.up * jumpForce, ForceMode.Impulse);           // Apply jump force
        }
        
    }

    void ResetJump()
    {
        readyToJump = true; // Allow the player to jump again
    }

    private bool OnSlopeCheck()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeCast, playerHeight * 0.5f + 0.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeCast.normal); // Calculate the slope angle relative to the upward direction
            return angle < maxSlopeAngle && angle > 2f;                // Check if the slope is walkable
        }
        return false;
    }

    private Vector3 GetSlopeDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeCast.normal).normalized; // Project movement direction onto the slope's plane
    }

    // WALLRUNNING
    void WallCheck()
    {
        wallLeft = Physics.Raycast(transform.position - new Vector3(0, playerHeight/4, 0),
            -orientation.right, out leftWallHit, wallCheckDist, wallLayer);

        wallRight = Physics.Raycast(transform.position - new Vector3(0, playerHeight / 4, 0),
            orientation.right, out rightWallHit, wallCheckDist, wallLayer);

        //debug rays to visualize the wall checks
        Debug.DrawRay(
            transform.position - new Vector3(0, playerHeight / 4, 0),
            -orientation.right * wallCheckDist,
            wallLeft ? Color.green : Color.red
        );

        Debug.DrawRay(
            transform.position - new Vector3(0, playerHeight / 4, 0),
            orientation.right * wallCheckDist,
            wallRight ? Color.green : Color.red
        );
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundLayer);
    }

    void StartWallrunning()
    {
        Debug.Log("Start Wallrun");
        movementState = MovementState.wallrunning;
        ResetJump();
        if (wallLeft)
        {
            Debug.Log("Tilt left");
            camScript.WallrunTiltLeft();
        } else
        {
            Debug.Log("Tilt right");
            camScript.WallrunTiltRight();
        }
    }
    void StopWallrunning()
    {
        Debug.Log("Stop Wallrun");

        _rbody.useGravity = true;
        camScript.ResetCameraEffects(true);
    }

    public void StartGrapple(Vector3 grapplePoint)
    {
        StartCoroutine(GrappleCouroutine(grapplePoint));
    }

    public IEnumerator GrappleCouroutine(Vector3 grapplePoint)
    {
        lineRenderer.enabled = true; //turn on rope rendering
        lineRenderer.positionCount = 2;
        currentGrapplePoint = grapplePoint;
        Vector3 forceDirection;

        //goal length is set shorter than start length to give initial pull 
        float grappleLength = Vector3.Distance(transform.position, currentGrapplePoint) * grappleYankPercentDistance; 

        while (isGrappled) //while still grappling
        {
            yield return new WaitForFixedUpdate(); //every fixedupdate
            forceDirection = (grapplePoint - transform.position).normalized; //get a direction pointing from player to grapple point
            Debug.Log("Grapple Point: " + grapplePoint);

            float distanceFromProperLength = (grappleLength - Vector3.Distance(transform.position, currentGrapplePoint)); //calculate how far from proper length the grapple is

            _rbody.AddForce(forceDirection * grappleForce * 100 * -distanceFromProperLength / grappleStretch, ForceMode.Force); //apply a force to keep the player within that length


            //Debug.DrawLine(transform.position, grapplePoint, Color.black, Time.fixedDeltaTime);

            
        }
        //when player stops grappling, turn off rope renderer.
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;


        yield return null;

    }
}