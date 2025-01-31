using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraControllerScript : MonoBehaviour
{
    public GameObject player;
    PlayerControllerScript playerControllerScript;
    public float sensitivityX = 2f;
    public float sensitivityY = 2f;
    float mouseX;
    float mouseY;
    float yAngle;
    float xAngle;
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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = GetComponent<Camera>();
        playerControllerScript = player.GetComponent<PlayerControllerScript>();
        ResetCameraEffects(false);
        
    }

    private void Update()
    {
        // Apply mouse movement directly without deltaTime
        yAngle -= mouseY * sensitivityY; // Negative because mouse Y moves camera y in opposite direction
        xAngle += mouseX * sensitivityX;

        // Clamp the vertical rotation
        yAngle = Mathf.Clamp(yAngle, -90f, 90f);

        // Apply rotations
        transform.rotation = Quaternion.Euler(yAngle, xAngle, 0);
        player.transform.rotation = Quaternion.Euler(0, xAngle, 0);
        orientation.transform.rotation = Quaternion.Euler(0, xAngle, 0);
        UpdateCameraEffects();
        
    }

    void OnLookX(InputValue value)
    {
        mouseX = value.Get<float>();
    }

    void OnLookY(InputValue value)
    {
        mouseY = value.Get<float>();
    }


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
        Quaternion baseRotation = Quaternion.Euler(yAngle, xAngle, 0);
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

    public void GrappleCheck()
    {
        Debug.Log("Grappling");
        Physics.Raycast(transform.position, transform.forward, out grappleRangeFinder, grappleRange, grappleLayer);
        if (grappleRangeFinder.collider != null)
        {
            Debug.DrawLine(transform.position, grappleRangeFinder.point, Color.green);
            Debug.Log("Hit: " + grappleRangeFinder.collider.name);
            playerControllerScript.isGrappled = true;
            playerControllerScript.StartGrapple(grappleRangeFinder.point);
            
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward*grappleRange, Color.red);
        }
    }
}