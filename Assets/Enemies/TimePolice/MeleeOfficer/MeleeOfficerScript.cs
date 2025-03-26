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

    [SerializeField] ParticleSystem debugAttackParticle;
    [SerializeField] bool showDebugAttackParticle = true;

    [SerializeField] Transform attackPosition;

    Rigidbody rb;
    Vector3 directionToTarget;
    EntityStats es;

    Quaternion targetRotation;

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
        directionToTarget = (target.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 200 * Time.fixedDeltaTime);
        if (Vector3.Distance(target.transform.position, transform.position) > visibilityDistance)
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

        if(es.isDead)
        {
            Die();
        }
    }

    void AttackSphere()
    {
        if(showDebugAttackParticle)
        {
            debugAttackParticle.Play();
        }
        Collider[] colliders = Physics.OverlapSphere(attackPosition.position, es.getAttackRange());

        foreach (Collider col in colliders)
        {
            //Debug.Log($"checking: {col.gameObject.name}");
            if (col.gameObject != gameObject && col.gameObject.CompareTag("Player")) // Exclude self
            {
                col.gameObject.GetComponent<EntityStats>().InflictDamage(es.getDamage());
            }
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    IEnumerator Behavior()
    {
        while(true)
        {
            if (decisionRange == Range.unaware)
            {
                yield return StartCoroutine(Meander());
            }

            else if (decisionRange == Range.aware)
            {
                yield return StartCoroutine(WalkTowardsPlayer());
            }

            else if (decisionRange == Range.inRange)
            {
                yield return StartCoroutine(Attacking());
            }

            yield return null;
        }
    }

    IEnumerator Attacking() // 3
    {
        while (decisionRange == Range.inRange)
        {
            targetRotation = Quaternion.LookRotation(GetXZDirectionToPlayer());
            yield return new WaitForSeconds(es.getAtkDelay());

            AttackSphere();
            yield return null;
        }
    }

    IEnumerator WalkTowardsPlayer()
    {
        //print("Walking");
        while(decisionRange == Range.aware)
        {
            if(decisionRange != Range.aware)
            {
                yield break;
            }

            //Quaternion targetRotation = Quaternion.LookRotation(GetXZDirectionToPlayer());
            //StartCoroutine(RotateTowardsDirection(GetXZDirectionToPlayer()));

            targetRotation = Quaternion.LookRotation(GetXZDirectionToPlayer());
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 500 * Time.deltaTime);

            rb.velocity = new Vector3(directionToTarget.normalized.x * es.getSpeed(), rb.velocity.y, directionToTarget.normalized.z * es.getSpeed());
            yield return null;
        }
    }

    Vector3 GetXZDirectionToPlayer()
    {
        Vector3 dir = (target.transform.position - transform.position).normalized;

        return new Vector3(dir.x, 0, dir.z);
    }

    IEnumerator Meander() // 1
    {
        //print("Meandering");
        targetRotation = Quaternion.LookRotation(GetFarthestDistanceDirection());
        while(decisionRange == Range.unaware)
        {
            yield return StartCoroutine(MoveForward());
            yield return null;
        }
    }

    IEnumerator MoveForward() // 2
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
            Debug.DrawRay(transform.position, forward * wallCheckDistance, Color.white, 0.1f);
            rb.velocity = new Vector3(transform.forward.normalized.x * es.getSpeed(), rb.velocity.y, transform.forward.normalized.z * es.getSpeed());
            forward = transform.forward;
            yield return null;
        }
        targetRotation = Quaternion.LookRotation(GetFarthestDistanceDirection());
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

        Debug.DrawRay(origin, forward * maxDistance, Color.green, 3f);
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
            Debug.DrawRay(origin, Quaternion.Euler(0, i, 0) * forward * maxDistance, Color.green, 10f);
            if (Physics.Raycast(origin, Quaternion.Euler(0, i, 0) * forward, out hit, maxDistance))
            {
                if(hit.distance > maxDistanceFound)
                {
                    maxDistanceFound = hit.distance;
                    bestDirection = Quaternion.Euler(0, i, 0) * forward;
                }
            }
            else if(Random.Range(0, 3) == 0)
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
