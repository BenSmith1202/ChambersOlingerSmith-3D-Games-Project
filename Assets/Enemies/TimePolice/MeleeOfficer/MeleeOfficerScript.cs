using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeOfficerScript : MonoBehaviour
{
    [SerializeField] GameObject target;

    [SerializeField] float meleeRange = 1;
    [SerializeField] int angleCheckDelta = 30;

    Rigidbody rb;
    Vector3 directionToTarget;
    EntityStats es;

    private void Start()
    {
        es = GetComponent<EntityStats>();
        rb = GetComponent<Rigidbody>();
        if (target == null)
        {
            target = GameObject.Find("Player");
        }
        getFarthestDistanceDirection();
    }

    private void FixedUpdate()
    {
        //try to get to target
        if(Vector3.Distance(target.transform.position, transform.position) < meleeRange)
        {
            //attack
        }
        else
        {
            directionToTarget = (target.transform.position - transform.position).normalized;
            rb.velocity = new Vector3(directionToTarget.x * es.baseSpeed, rb.velocity.y, directionToTarget.z * es.baseSpeed);
        }
    }

    /*
     * This method casts some rays out from the enemy
     * it picks the longest ray
     * this ray direction is the nicest direction to walk towards for the enemy 
     */
    Vector3 getFarthestDistanceDirection()
    {
        Vector3 origin = transform.position;
        float maxDistance = 100;
        RaycastHit hit;

        Vector3 forward = transform.forward;
        //Vector3 forward45 = Quaternion.Euler(0, 45, 0) * forward;

        Vector3 bestDirection = forward;
        float maxDistanceFound;

        Debug.DrawRay(origin, forward * maxDistance, Color.red, 10f);
        if (Physics.Raycast(origin, forward, out hit, maxDistance))
        {
            maxDistanceFound = hit.distance;
        }
        else
        {
            maxDistanceFound = maxDistance;
            return bestDirection;
        }

        for(int i = angleCheckDelta; i <= 360; i += angleCheckDelta)
        {
            Debug.DrawRay(origin, Quaternion.Euler(0, angleCheckDelta, 0) * forward * maxDistance, Color.red, 10f);
            if (Physics.Raycast(origin, Quaternion.Euler(0, angleCheckDelta, 0) * forward, out hit, maxDistance))
            {
                if(hit.distance > maxDistanceFound)
                {
                    maxDistanceFound = hit.distance;
                    bestDirection = Quaternion.Euler(0, angleCheckDelta, 0) * forward;
                }
            }
            else
            {
                maxDistanceFound = maxDistance;
                bestDirection = Quaternion.Euler(0, angleCheckDelta, 0) * forward;
                return bestDirection;
            }
        }

        //Debug.DrawRay(origin, forward * maxDistance, Color.red, 10f);

        return bestDirection;
    }
}
