using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraControllerScript : MonoBehaviour
{
    public GameObject player;
    PlayerControllerScript pm;
    public float sensitivityX = 2f;
    public float sensitivityY = 2f;
    float mouseX;
    float mouseY;
    float xRotation;
    float yRotation;
    public Transform orientation;
    Camera cam;
    public float fov;
    public float dashFOV;
    public float wallrunFOV;
    public float wallrunTilt;
    private float targetFOV;
    private float targetTilt;
    public float fovSpeed;
    public float tiltSpeed;
    float currentTilt;

    RaycastHit grappleRangeFinder;
    public LayerMask grappleLayer;
    public float grappleRange;
    private InputAction grappleAction;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = GetComponent<Camera>();
        pm = player.GetComponent<PlayerControllerScript>();
        grappleAction = GetComponent<PlayerInput>().actions["Grapple"]; // Get the crouch input action
        ResetCameraEffects(false);
        
    }

    private void Update()
    {
        // Apply mouse movement directly without deltaTime
        xRotation -= mouseY * sensitivityY; // Negative because mouse Y moves camera X in opposite direction
        yRotation += mouseX * sensitivityX;

        // Clamp the vertical rotation
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotations
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        player.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        UpdateCameraEffects();
        if (grappleAction.IsPressed() && !pm.isGrappled)
        {   
            GrappleCheck();
        }
        if (!grappleAction.IsPressed() && pm.isGrappled)
        {
            pm.isGrappled = false;
        }
    }

    void OnLookX(InputValue value)
    {
        mouseX = value.Get<float>();
    }

    void OnLookY(InputValue value)
    {
        mouseY = value.Get<float>();
    }

    //void OnGrapple(InputValue value)
    //{
    //    if (value.isPressed)
    //    {
    //        print("Grapple");
    //        grappling = true;
    //    } else
    //    {
    //        print("ungrapple");
    //        grappling = false;
    //    }
    //}

    // Then modify your tilt functions to set the targets
    public void WallrunTiltLeft()
    {
        targetFOV = wallrunFOV;
        targetTilt = wallrunTilt;
    }

    public void WallrunTiltRight()
    {
        targetFOV = wallrunFOV;
        targetTilt = -wallrunTilt;
    }

    public void DashZoom()
    {
        Debug.Log("Dash Fov");

        targetFOV = dashFOV;
    }

    private void UpdateCameraEffects()
    {
        // Smooth FOV transition
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSpeed);

        // Smooth tilt transition
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        // Apply tilt to the camera's local Z rotation
        Quaternion baseRotation = Quaternion.Euler(xRotation, yRotation, 0);
        Quaternion tiltRotation = Quaternion.Euler(0, 0, currentTilt);
        cam.transform.rotation = baseRotation * tiltRotation;
    }

    public void ResetCameraEffects(bool onlyResetTilt)
    {
        Debug.Log("reset camera");
        if (!onlyResetTilt)
        {
            targetFOV = fov;       // Reset field of view
        }
        targetTilt = 0f;       // Reset tilt
    }

    void GrappleCheck()
    {
        Debug.Log("Grappling");
        Physics.Raycast(transform.position, transform.forward, out grappleRangeFinder, grappleRange, grappleLayer);
        if (grappleRangeFinder.collider != null)
        {
            Debug.DrawLine(transform.position, grappleRangeFinder.point, Color.green);
            Debug.Log("Hit: " + grappleRangeFinder.collider.name);
            pm.isGrappled = true;
            pm.StartGrapple(grappleRangeFinder.point);
            
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward*grappleRange, Color.red);
        }
    }
}