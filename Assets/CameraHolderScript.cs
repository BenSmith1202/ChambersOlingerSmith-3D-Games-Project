using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHolderScript : MonoBehaviour
{
    public GameObject cameraPosition;
    public float camSmoothingSpeed;
    Vector3 zero = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = cameraPosition.transform.position;
        transform.position = Vector3.SmoothDamp(transform.position, cameraPosition.transform.position, ref zero, camSmoothingSpeed);
    }
}
