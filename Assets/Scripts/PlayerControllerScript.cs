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
    public float runSpeed;     // Walking speed of the player
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

    public LayerMask wallLayer; // Layer used to determine what walls can be run on
    public float wallrunForce;  // effectively wall run speed
    public float maxWallTime;   // Maximum time in seconds that a player can run on walls before touching the ground
    float wallRunTimer;         // Time counter for above
    public float wallJumpVerticalForce;    // Vertical component of wall jump force vector
    public float wallJumpHorizontalForce;  // Horizontal component of wall jump force vectors
    public float wallrunSpeed;   // Speed during wallrun
    public float wallrunGravityCounter; // controls how much the player falls while wall running. if < 0: increase gravity, if < 1: reduce gravity, if > 1: reverse gravity


    [Header("Wall Detection")]

    public float wallCheckDist; // Length of wall detection raycast
    public float minJumpHeight; // Minimum heigh above the ground to start a wallrun
    private RaycastHit leftWallHit;  // These two are to store the results of the above raycast
    private RaycastHit rightWallHit;
    private bool wallLeft;      // True if on a wall to the left
    private bool wallRight;     // True if on a wall to the right


    [Header("Grappling")] //DOES NOT WORK ON MOVING OBJECTS
    public float grappleCooldownTime;
    float grappleCooldown = 0;

    public bool isGrappled;     // true iff grapple button is held down and a successful grapple is in progress
    public float grappleForce;  // force of the ropes tension
    public float grappleStretch;    // divides the above, how stretchy the rope is
    public float grappleReelSpeed;  // how fast the max length of the grapple decreases (linearly)
    public float grappleYankPercentDistance; // percent of player's distance from the grapple point that the rope length is initialized to
    public float swingSpeed;                 // The speed at which the player influences their momentum with the movement keys
    public float maxSwingVelocity;           // the speed cap for grappling.
    public Vector3 currentGrapplePoint;      // the location of the current grapple point
    LineRenderer lineRenderer; // Reference to the line Renderer for the grapple rope.
    public AudioClip grappleSound;

    [Header("Dash Slam")]
    public float dashCooldownTime;
    float dashCooldown = 0;
    public float dashDuration;
    public float dashSpeed;
    public float postDashSpeedReduction;
    public float minPostDashSpeed;


    [Header("References")]
    public GameObject shootIcon;
    public GameObject grappleIcon;
    public GameObject dashIcon;

    //PLACEHOLDER PLEASE DELETE
    public float shootCooldownTime;
    public float shootCooldown = 0;

    //public GameObject player;
    private Rigidbody _rbody;   // Reference to the player's Rigidbody component
    private CapsuleCollider _collider;  // Reference to the player's CapsuleCollider component
    private Vector2 moveVal;    // Stores movement input values
    //private InputAction sprintAction;  // Reference to the sprint input action
    private InputAction crouchAction;  // Reference to the crouch input action
    private InputAction dashAction;
    private InputAction grappleAction;
    Vector3 moveDirection;      // Calculated movement direction
    public Transform orientation; // Reference to your camera's orientation
    public GameObject cam;
    CameraControllerScript camScript;
    AudioSource audioSource;

    [Header("Particles")]
    public ParticleSystem grappleParticles;


    // State machine states
    public enum MovementState
    {
        running,    // Player is walking
        //sprinting,  // Player is sprinting
        wallrunning,// Player is wallrunning
        crouching,  // Player is crouching
        freefall,       // Player is in the air
        swinging,   //player is on a grappling hook, or has just departed from a hook and yet to touch the ground or wall
        dashing
    }


    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        _collider = GetComponent<CapsuleCollider>(); 
        _rbody = GetComponent<Rigidbody>(); 
        //sprintAction = GetComponent<PlayerInput>().actions["Sprint"]; // Get the sprint input action
        crouchAction = GetComponent<PlayerInput>().actions["Crouch"]; // Get the crouch input action
        grappleAction = GetComponent<PlayerInput>().actions["Grapple"]; // Get the grapple input action
        dashAction = GetComponent<PlayerInput>().actions["Dash"]; // Get the crouch input action
        startYScale = transform.localScale.y; // Store the original Y scale of the player
        camScript = cam.GetComponent<CameraControllerScript>();
        lineRenderer.enabled = false;
        audioSource = GetComponent<AudioSource>();
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
        HandleCooldowns();
        WallCheck();
        CheckGrapple();
    }


    private void HandleCooldowns()
    {
        shootCooldown -= Time.deltaTime; // Not Yet Implemented

        grappleCooldown -= Time.deltaTime; //handled in the StartGrapple method

        dashCooldown -= Time.deltaTime;

        if (dashAction.IsPressed() && dashCooldown < 0)
        {
            StartCoroutine(DashSlam());
            dashIcon.GetComponent<AbilityIconScript>().StartCooldown(dashCooldownTime);
            dashCooldown = dashCooldownTime;
        }

    }


    //Defines transitions between states
    private void StateHandler()
    {

        // PRIORITY 0: Grappling - if on hook, no other state is possible than swinging
        if (isGrappled)
        {
            movementState = MovementState.swinging;
        }
        // PRIORITY 1: Crouching - Highest priority state when grounded
        else if (crouchAction.IsPressed() && grounded)
        {
            // Modify player scale and apply downward force if entering crouch state
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
            // Reset player scale when not crouching
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

            // PRIORITY 2: Wallrunning - Requires specific conditions
            if ((wallLeft || wallRight) && moveVal.y > 0.1f && AboveGround())
            {
                if (movementState != MovementState.wallrunning)
                {
                    StartWallrunning();
                    speed = wallrunSpeed;
                }
            }
            // PRIORITY 3: Ground movement
            else if (grounded)
            {
                if (movementState != MovementState.running)
                {
                    camScript.ResetCameraEffects(false);
                    movementState = MovementState.running;
                    speed = runSpeed;
                }
            }
            // PRIORITY 4: Air movement
            else
            {
                // Reset camera effects for air states, except when already swinging
                if (movementState != MovementState.swinging && movementState != MovementState.dashing)
                {
                    camScript.ResetCameraEffects(true);
                    movementState = MovementState.freefall;
                }
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


    // depending on what movment state the player is in, apply different movement
    //Activates every fixed update
    private void HandleMovement()
    {
        // PRIORITY 1: Slope Movement - Special physics handling
        if (OnSlopeCheck())
        {
            _rbody.AddForce(10f * speed * GetSlopeDirection(), ForceMode.Force);
            _rbody.AddForce(slopeCling * -slopeCast.normal, ForceMode.Force);
            _rbody.useGravity = false;
            return;
        }

        // Movement force application based on current state
        Vector3 movementForce = 10f * moveDirection.normalized;
        switch (movementState)
        {
            case MovementState.crouching:
                _rbody.AddForce(movementForce * crouchSpeed, ForceMode.Force);
                break;

            case MovementState.running:
                _rbody.AddForce(movementForce * speed, ForceMode.Force);
                break;

            case MovementState.wallrunning:
                HandleWallrunMovement();
                break;

            case MovementState.freefall:
                HandleFreefallMovement(movementForce);
                break;

            case MovementState.swinging:
                HandleSwingingMovement(movementForce);
                break;
        }

        // Speed control
        ControlMovementSpeed();

        // Restore gravity when not on slope
        _rbody.useGravity = true;
    }


    private void HandleWallrunMovement()
    {
        _rbody.useGravity = false;
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        wallForward = (orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude
            ? -wallForward
            : wallForward;

        // Wall cling for convex walls
        if ((wallLeft && moveVal.x < 0) || (wallRight && moveVal.x > 0))
        {
            _rbody.AddForce(-wallNormal * 100, ForceMode.Force);
        }

        _rbody.AddForce(100f * wallrunForce * wallForward, ForceMode.Force);
        _rbody.AddForce(25f * wallrunGravityCounter * transform.up, ForceMode.Force);
    }


    private void HandleFreefallMovement(Vector3 movementForce)
    {
        Vector3 xzVel = new(_rbody.velocity.x, 0f, _rbody.velocity.z);

        if (xzVel.magnitude > speed)
        {
            _rbody.AddForce(RemovePositiveParallelComponent(0.8f * airMultiplier * speed * movementForce, xzVel), ForceMode.Force);
        } else
        {
            _rbody.AddForce(0.8f * airMultiplier * speed * movementForce, ForceMode.Force);
        }
        
    }


    private void HandleSwingingMovement(Vector3 movementForce)
    {        
        if (isGrappled) //if currently grappled to an object
        {               // we consider the full 3d velocity.
            if (_rbody.velocity.magnitude > maxSwingVelocity) //if its greater than the max:
            {
                // apply movement while removing any component in the direction of velocity
                _rbody.AddForce(RemovePositiveParallelComponent(movementForce * swingSpeed, _rbody.velocity), ForceMode.Force);
            }
            else //otherwise (if less than max)
            {
                //apply movment like normal
                _rbody.AddForce(movementForce * swingSpeed, ForceMode.Force);
            }
        }
        else // if in freefall after a grapple
        {
            // do the same thing only considering horizontal velocity
            Vector3 xzVel = new(_rbody.velocity.x, 0f, _rbody.velocity.z); // Horizontal velocity

            if (xzVel.magnitude > maxSwingVelocity)
            {
                _rbody.AddForce(RemovePositiveParallelComponent(0.5f * airMultiplier * speed * movementForce, xzVel), ForceMode.Force);
            }
            else
            {
                _rbody.AddForce(0.5f * airMultiplier * speed * movementForce, ForceMode.Force);
            }
        }
        
    }


    private void ControlMovementSpeed()
    {

        Vector3 xzVel = new(_rbody.velocity.x, 0f, _rbody.velocity.z);

        if (xzVel.magnitude > speed && movementState != MovementState.swinging && movementState != MovementState.freefall && movementState != MovementState.dashing) 
        {
            Vector3 cappedVel = xzVel.normalized * speed;
            _rbody.velocity = new Vector3(cappedVel.x, _rbody.velocity.y, cappedVel.z);
        }
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

    void CheckGrapple()
    {
        if (grappleAction.IsPressed() && !isGrappled)
        {
            if (grappleCooldown < 0)
            {
                camScript.GrappleCheck();
            }
        }
        if (!grappleAction.IsPressed() && isGrappled)
        {
            isGrappled = false;
        }

    }
    public void StartGrapple(Vector3 grapplePoint)
    {
            grappleCooldown = grappleCooldownTime;
            grappleIcon.GetComponent<AbilityIconScript>().StartCooldown(grappleCooldownTime);
            StartCoroutine(GrappleCouroutine(grapplePoint));
    }


    public IEnumerator GrappleCouroutine(Vector3 grapplePoint)
    {
        lineRenderer.enabled = true; //turn on rope rendering
        lineRenderer.positionCount = 2;
        currentGrapplePoint = grapplePoint;
        ParticleSystem grappleParticleInstance = Instantiate(grappleParticles, currentGrapplePoint, Quaternion.identity);
        Destroy(grappleParticleInstance, 1f);
        Vector3 forceDirection;
        AudioSource.PlayClipAtPoint(grappleSound, grapplePoint);

        //Length of rope
        //goal length is set shorter than start length to give initial pull 
        float grappleLength = Vector3.Distance(transform.position, currentGrapplePoint) * grappleYankPercentDistance; 

        while (isGrappled) //while still grappling
        {
            yield return new WaitForFixedUpdate(); //every fixedupdate
            forceDirection = (grapplePoint - transform.position).normalized; //get a direction pointing from player to grapple point
            movementState = MovementState.swinging;
            Debug.Log("Grapple Point: " + grapplePoint);

            float distanceBeyondMaxLength = (Vector3.Distance(transform.position, currentGrapplePoint) - grappleLength); //Distance from grapple point further than the rope length

            //apply a tensile force to keep the player within that length
            if (distanceBeyondMaxLength > 0)
            {
                _rbody.AddForce(100f * distanceBeyondMaxLength * grappleForce * forceDirection / grappleStretch, ForceMode.Force); 
            }

            // reel in grapple rope over time
            grappleLength -= Time.fixedDeltaTime * grappleReelSpeed;
        }
        //when player stops grappling, turn off rope renderer.
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
        yield return null;
    }

    public IEnumerator DashSlam()
        /** This dash logic is just a prototype of what i though would be cool. Feel free to comment it out or change it 
         * -Ben
         **/
    {
        MovementState prevMoveState = movementState; // save previous move state
        movementState = MovementState.dashing;       // start dashing
        float dashTime = dashDuration;               //start dash timer
        Vector3 startVelocity = _rbody.velocity;     // save starting velocity
        _rbody.velocity = Vector3.zero;              //kill velocity
        Vector3 dashDirection = cam.transform.forward;
        camScript.DashZoom();
        _rbody.useGravity = false;

        // as long as the time hasn't run out and the player hasnt caused another movement state (by grappling or hitting a wall/floor)
        while (movementState == MovementState.dashing && dashTime > 0f)
        {
            _rbody.velocity = dashSpeed * dashDirection; //dash in the direction of the camera
            dashTime -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        camScript.ResetCameraEffects(false);
        _rbody.useGravity = true;

        if (movementState == MovementState.dashing) //as long as the player didnt start a new movment state
        {
            movementState = prevMoveState; //return it to what it was
        }
        
        // After a dash, go some fraction of your initial speed defined by postDash speed reduction
        // but go no slower than minimum post dashVelocity
        // and no greater than the mid-dash velocity
        // hopefully this feels good
        Vector3 newReducedVelocity = (Mathf.Max(Mathf.Min(postDashSpeedReduction * startVelocity.magnitude, dashSpeed), minPostDashSpeed) * dashDirection);
        _rbody.velocity = newReducedVelocity;

        yield return null;
    }


    //UTILITIES

    
    Vector3 RemovePositiveParallelComponent(Vector3 vectorIn, Vector3 referenceVector)
    /**Takes a vector input and a reference vector, and returns the input vector minus any positive 
     ** parallel component to the reference vector.**/
    {
        // normalize the reference direction into a unit vector
        Vector3 normalizedReference = referenceVector.normalized;

        // project vectorToModify onto the reference direction
        // this gives us the component of vectorToModify that is parallel to the reference
        Vector3 parallelComponent = Vector3.Dot(vectorIn, normalizedReference) * normalizedReference;

        // figure out if the parallel component is in the same or opposite direction as the reference
        float parallelSign = Mathf.Sign(Vector3.Dot(parallelComponent, referenceVector));


        if (parallelSign > 0) //if there is a positive component parallel to the reference
        {
            return vectorIn - parallelComponent; //remove it
        } else //otherwise
        {
            return vectorIn; //return original
        }
    }
}