using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;
using UnityEngine.InputSystem.HID;

public class MeleeOfficerScript : MonoBehaviour
{
    [SerializeField] GameObject target;

    [SerializeField] int angleCheckDelta = 30;

    [SerializeField] float minWalkTimeLimit = 2;
    [SerializeField] float maxWalkTimeLimit = 5;

    [SerializeField] float visibilityDistance = 30;
    [SerializeField] float meleeRange = 1;

    Rigidbody rb;
    Vector3 directionToTarget;
    EntityStats es;

    enum Range
    {
        unaware, aware, inRange
    }
    Range decisionRange;

    private void Start()
    {
        es = GetComponent<EntityStats>();
        rb = GetComponent<Rigidbody>();
        if (target == null)
        {
            target = GameObject.Find("Player");
        }

        decisionRange = Range.unaware;
        StartCoroutine(Behavior());
    }

    private void FixedUpdate()
    {
        if(Vector3.Distance(target.transform.position, transform.position) > visibilityDistance)
        {
            decisionRange = Range.unaware;
        }
        else if(Vector3.Distance(target.transform.position, transform.position) > meleeRange)
        {
            decisionRange = Range.aware;
        }
        else
        {
            decisionRange = Range.inRange;
        }
    }

    IEnumerator Behavior()
    {
        while(true)
        {
            yield return StartCoroutine(Meander());
        }
    }

    IEnumerator Meander()
    {
        yield return StartCoroutine(RotateTowardsDirection(GetFarthestDistanceDirection()));
        while(decisionRange == Range.unaware)
        {
            yield return StartCoroutine(MoveForward());
        }
    }

    IEnumerator RotateTowardsDirection(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 500 * Time.deltaTime);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    IEnumerator MoveForward()
    {
        float wallCheckDistance = 3;
        RaycastHit hit;
        Vector3 forward = transform.forward;

        float maxWalkTime = Time.time + Random.Range(minWalkTimeLimit, maxWalkTimeLimit);
        while (!Physics.Raycast(transform.position, forward, out hit, wallCheckDistance) && Time.time < maxWalkTime)
        {
            if(decisionRange != Range.unaware)
            {
                yield break;
            }
            Debug.DrawRay(transform.position, forward * wallCheckDistance, Color.blue, 0.1f);
            rb.velocity = new Vector3(transform.forward.normalized.x * es.baseSpeed, rb.velocity.y, transform.forward.normalized.z * es.baseSpeed);
            yield return null;
        }
        yield return StartCoroutine(RotateTowardsDirection(GetFarthestDistanceDirection()));
    }

    /*
     * This method casts some rays out from the enemy
     * it picks the longest ray
     * this ray direction is the nicest direction to walk towards for the enemy 
     */
    Vector3 GetFarthestDistanceDirection()
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
            Debug.DrawRay(origin, Quaternion.Euler(0, i, 0) * forward * maxDistance, Color.red, 10f);
            if (Physics.Raycast(origin, Quaternion.Euler(0, i, 0) * forward, out hit, maxDistance))
            {
                if(hit.distance > maxDistanceFound)
                {
                    maxDistanceFound = hit.distance;
                    bestDirection = Quaternion.Euler(0, i, 0) * forward;
                }
            }
            else
            {
                maxDistanceFound = maxDistance;
                bestDirection = Quaternion.Euler(0, i, 0) * forward;
                return bestDirection;
            }
        }

        //Debug.DrawRay(origin, forward * maxDistance, Color.red, 10f);

        return bestDirection;
    }
}
