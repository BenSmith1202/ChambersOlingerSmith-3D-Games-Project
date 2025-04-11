using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MechScript : MonoBehaviour
{
    GameObject player;
    Rigidbody rb;
    MeshRenderer mr;
    EntityStats es;

    [SerializeField] GameObject missle;
    [SerializeField] Vector3 missleLaunchOffset;

    [SerializeField] float visibilityDistance;
    [SerializeField] int rotateSpeed = 30;
    [SerializeField] float desiredDistanceToPlayer = 25;

    float nextFireTime = 0;

    private void Start()
    {
        es = GetComponent<EntityStats>();
        mr = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        StartCoroutine(Behavior());
    }

    IEnumerator Behavior()
    {
        while(true)
        {
            if(GetDistanceToPlayer() < visibilityDistance)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, GetRotationToPlayer(), rotateSpeed * Time.deltaTime);

                //print("Y Rot: " + transform.rotation.y + " | Desired Y Rot: " + GetRotationToPlayer().y + " | Their Difference: " + Mathf.Abs(transform.rotation.y - GetRotationToPlayer().y));

                if(Mathf.Abs(transform.rotation.y - GetRotationToPlayer().y) < 0.2f) //if we're facing player (with forgiveness threshold)
                {
                    if(GetDistanceToPlayer() > desiredDistanceToPlayer)
                    {
                        rb.velocity = new Vector3(-transform.forward.normalized.x * es.getSpeed(), rb.velocity.y, -transform.forward.normalized.z * es.getSpeed());
                    }
                    else if(GetDistanceToPlayer() + 1 < desiredDistanceToPlayer)
                    {
                        print("else is happeneing");
                        rb.velocity = new Vector3(transform.forward.normalized.x * es.getSpeed(), rb.velocity.y, transform.forward.normalized.z * es.getSpeed());
                    }

                    if(nextFireTime < Time.time)
                    {
                        FireMissle();
                    }
                }
                else
                {
                    rb.velocity = Vector3.Slerp(rb.velocity, new Vector3(0, rb.velocity.y, 0), 1);
                }
            }
            else
            {

            }
            yield return null;
        }
    }

    void FireMissle()
    {
        nextFireTime = Time.time + es.getAtkDelay() + Random.Range(0f, 1f);
        Instantiate(missle, transform.position + missleLaunchOffset, GetRotationToPlayer() * Quaternion.Euler(90, 0 , 0));
    }

    float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.transform.position);
    }

    Quaternion GetRotationToPlayer()
    {
        return Quaternion.LookRotation(GetXZDirectionToPlayer(), Vector3.up);
    }

    Vector3 GetXZDirectionToPlayer()
    {
        Vector3 dir = (transform.position - player.transform.position).normalized;

        return new Vector3(dir.x, 0, dir.z);
    }
}
