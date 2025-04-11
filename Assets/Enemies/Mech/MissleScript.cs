using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleScript : MonoBehaviour
{
    [SerializeField] float missleSpeed;

    GameObject target;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        target = GameObject.Find("Player");

        rb.velocity = Vector3.Slerp(rb.velocity, GetDirectionToPlayer() * missleSpeed, 0.3f);
    }

    Vector3 GetDirectionToPlayer()
    {
        return (target.transform.position - transform.position).normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
