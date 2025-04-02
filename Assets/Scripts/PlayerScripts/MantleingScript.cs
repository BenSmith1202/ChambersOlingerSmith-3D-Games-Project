using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
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
        }
    }

    private void MantleCheck()
    {
        if (Input.GetKey(KeyCode.Space)) //TODO: Turn this into a new input system based approach
        {
            //fire out a raycast from your toes to see if you are standing in front of a wall
            Vector3 mantleDirection = Quaternion.Euler(maxSlopeAngle, 0 , 0) * cam.transform.forward ;
            Vector3 toesPosition = transform.position - new Vector3(0, 0.5f * playerHeight, 0);

            if (Physics.Raycast(toesPosition, mantleDirection, out var firstHit, 1f, mantleLayer))
            {
                Debug.DrawRay(toesPosition, mantleDirection, Color.green, 0.02f);
                if (!Physics.Raycast(toesPosition + new Vector3(0, mantleHeight, 0), cam.transform.forward, 1f, mantleLayer))
                {
                    Debug.DrawRay(toesPosition + new Vector3(0, mantleHeight, 0), cam.transform.forward, Color.green, 0.02f);
                    isMantleing = true;

                }
                else
                {
                    isMantleing = false;
                }
                Debug.DrawRay(toesPosition + new Vector3(0, mantleHeight, 0), cam.transform.forward, Color.red, 0.02f);
                return;
            }
            else
            {
                isMantleing = false;
                Debug.DrawRay(toesPosition, mantleDirection, Color.red, 0.02f);
            }
        }
        else
        {
            isMantleing = false;
        }
    }
   
}