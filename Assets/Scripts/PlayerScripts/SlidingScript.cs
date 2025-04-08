using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerControllerScript;

public class SlidingScript : MonoBehaviour
{


    [Header("Crouching")]


    public float startYScale;   // Original Y scale of the player
    public float crouchYScale;  // Y scale while crouching
    public float crouchForce;   // Downward force applied when crouching
    float slideForce;
    public float slideBoost;//what fraction of movement speed is applied as a boost on slide
    public float slideDrag; 
    private float groundDrag;
    private InputAction crouchAction;  // Reference to the crouch input action
    public bool isCrouching;
    Rigidbody rb;
    PlayerControllerScript pcs;
    EntityStats stats;

    // Start is called before the first frame update
    void Start()
    {
        startYScale = transform.localScale.y; // Store the original Y scale of the player
        crouchAction = GetComponent<PlayerInput>().actions["Crouch"]; // Get the crouch input action
        pcs = GetComponent<PlayerControllerScript>();
        stats = GetComponent<EntityStats>();
        rb = GetComponent<Rigidbody>();
        slideForce = stats.getSpeed() * slideBoost;
    }

    private void Update()
    {
        CrouchCheck();
    }


    public void CrouchCheck()
    {
        if (crouchAction.IsPressed() && !pcs.inputPaused) 
        {
            //start crouch
            if (isCrouching == false)
            {
                //play crouch sound
                isCrouching = true;
                
                if (pcs.grounded)  //push player into the ground
                {
                    rb.AddForce(new Vector3(0, -crouchForce, 0), ForceMode.Impulse);
                    rb.AddForce(pcs.orientation.transform.forward * slideForce, ForceMode.VelocityChange);
                }
                groundDrag  = pcs.groundDrag; //save old drag
                pcs.groundDrag = slideDrag; //use new drag
                rb.drag = slideDrag;
                //start crouch
            }
            
            // Modify player scale and apply downward force if entering crouch state
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            
            pcs.movementState = MovementState.crouching;
            //pcs.speed = stats.crouchSpeed;
        } else
        {
            if (!isCrouching == false)
            {
                //play uncrouch sound
                isCrouching = false;
                //stop crouch
                pcs.groundDrag = groundDrag; //restore old drag
                rb.drag = groundDrag;
                // Reset player scale when not crouching
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
           
        }
        
    }

    public void HandleCrouching(Vector3 movementForce)
    {
        if (rb.velocity.magnitude < stats.crouchSpeed && pcs.grounded)
        {
            rb.AddForce(movementForce * stats.crouchSpeed, ForceMode.Force);
        }
    }
}
