using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class MantleingScript : MonoBehaviour
{
    // Start is called before the first frame update
    public LayerMask mantleLayer;
    Rigidbody rb;
    public Camera cam;
   
    private float playerHeight = 2f;
    private float playerRadius = 0.5f;
    private float maxSlopeAngle = 0;
    public float mantleCastLength = 1.5f;
    public float mantleForce = 0.2f;
    public float mantleSpeedReduction = 0.8f;
    public float mantleHeight = 0.5f;
    public bool isMantleing = false;

    


    void Start()
    {
        if (cam == null && GameObject.Find("Main Camera").GetComponent<Camera>())
        {
            print("Found main camera");
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        }
        else if(cam == null)
        {
            Debug.LogError("The player cannot find a camera for it's manteling script.");
        }

        rb = GetComponent<Rigidbody>();
        maxSlopeAngle = GetComponent<PlayerControllerScript>().maxSlopeAngle;
        playerHeight = GetComponent<CapsuleCollider>().height;
        playerRadius = GetComponent<CapsuleCollider>().radius;
    }

    // Update is called once per frame
    void Update()
    {
        MantleCheck();
    }

    private void FixedUpdate()
    {
        if (isMantleing)
        {
            Debug.Log("Applying mantle force");
            rb.velocity = new Vector3(rb.velocity.x, mantleForce, rb.velocity.z);
            rb.AddForce(transform.forward * 2, ForceMode.VelocityChange); //pushes the player onto the ledge to avoid hovering
        }
    }

    private void MantleCheck()
    {
        if (Input.GetKey(KeyCode.Space)) //TODO: Turn this into a new input system based approach
        {
            // Get the forward direction based on camera rotation (on the horizontal plane)
            Vector3 flatForward = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0) * Vector3.forward;

            // Apply a consistent upward angle to the forward direction
            // This creates a direction that points up at the maxSlopeAngle from the horizontal plane
            Vector3 mantleDirection = Quaternion.AngleAxis(maxSlopeAngle, Vector3.Cross(flatForward, Vector3.up).normalized) * flatForward;

            Vector3 toesPosition = transform.position - new Vector3(0, 0.5f * playerHeight, 0); //assumes player's center is in the middle

            if (Physics.Raycast(toesPosition, mantleDirection, out var firstHit, mantleCastLength, mantleLayer))
            {
                Debug.DrawRay(toesPosition, mantleDirection * mantleCastLength, Color.green, 0.02f);

                // Second check is there clearance above?
                if (!Physics.Raycast(toesPosition + new Vector3(0, mantleHeight, 0), flatForward, mantleCastLength, mantleLayer))
                {
                    Debug.DrawRay(toesPosition + new Vector3(0, mantleHeight, 0), flatForward * mantleCastLength, Color.green, 0.02f);
                    isMantleing = true;
                }
                else
                {
                    isMantleing = false;
                    Debug.DrawRay(toesPosition + new Vector3(0, mantleHeight, 0), flatForward * mantleCastLength, Color.red, 0.02f);
                }
                return;
            }
            else
            {
                isMantleing = false;
                Debug.DrawRay(toesPosition, mantleDirection * mantleCastLength, Color.red, 0.02f);
            }
        }
        else
        {
            isMantleing = false;
        }
    }

}