using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleScript : MonoBehaviour
{
    [SerializeField] GameObject explosion;

    [SerializeField] float initialSpeed = 5f;
    [SerializeField] float maxSpeed = 20f;
    [SerializeField] float acceleration = 2f;
    [SerializeField] float turnSpeed = 0.05f;
    [SerializeField] float fuelTime = 3;

    [SerializeField] GameObject particles;

    bool hasFuel = true;

    private float currentSpeed;
    GameObject target;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        target = GameObject.Find("Player");
        currentSpeed = initialSpeed;

        Invoke("NoFuel", fuelTime);
    }

    private void FixedUpdate()
    {
        if(hasFuel)
        {
            // Gradually increase speed up to the maximum
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);

            // First rotate toward the player
            Quaternion targetRotation = Quaternion.LookRotation(GetDirectionToPlayer());
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed);

            // Then set velocity in the forward direction with current speed
            rb.velocity = transform.forward * currentSpeed;
        }
        else
        {
            Quaternion targetRotation = Quaternion.LookRotation(-Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed);
        }
    }

    Vector3 GetDirectionToPlayer()
    {
        return (target.transform.position - transform.position).normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            return;
        }
        else
        {
            //should explode
        }
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void NoFuel()
    {
        hasFuel = false;
        particles.SetActive(false);
        rb.useGravity = true;
    }
}