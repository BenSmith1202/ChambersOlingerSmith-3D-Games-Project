using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunningScript : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public float wallRunForce;
    public float maxWallTime;
    public float wallRunTimer;

    [Header("Detection")]
    public float wallCheckDist;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")]
    //public GameObject player;
    public PlayerMovementControllerScript pm;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementControllerScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        WallCheck();
    }

    void WallCheck()
    {
        wallLeft = Physics.Raycast(transform.position, Vector3.left, out leftWallHit, wallCheckDist, wallLayer);
        wallRight = Physics.Raycast(transform.position, Vector3.right, out rightWallHit, wallCheckDist, wallLayer);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundLayer);
    }

    private void StateMachine()
    {

    }
}
