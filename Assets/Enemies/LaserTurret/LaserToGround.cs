using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserToGround : MonoBehaviour
{
    RaycastHit hit;

    private void Start()
    {
        if(Physics.Raycast(transform.position, -transform.up, out hit, 100))
        {
            print(hit.collider.gameObject.name);
            transform.position = hit.point + new Vector3(0, GetComponent<Collider>().bounds.extents.y, 0);
        }
    }
}
