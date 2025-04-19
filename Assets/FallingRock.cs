using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class FallingRock : MonoBehaviour
{
    [Header("Settings")]
    public GameObject landParticles;
    public int damage = 20;
    public float knockback = 5f;
    public float minDamageVelocity = 2f;

    private float hangTime;
    private float fallSpeed;
    private float lifetime;
    private EntityStats ownerStats;
    private bool isFalling;
    private bool hasLanded;
    private float spawnTime;

    public GameObject impactParticles;



    /// <summary>
    /// Initialize the falling rock
    /// </summary>
    public void Initialize(float hangTime, float fallSpeed, float lifetime, EntityStats owner)
    {
        this.hangTime = hangTime;
        this.fallSpeed = fallSpeed;
        this.lifetime = lifetime;
        ownerStats = owner;
        spawnTime = Time.time;

        StartCoroutine(FallRoutine());
        Destroy(gameObject, lifetime + hangTime + 1f);
    }

    private IEnumerator FallRoutine()
    {
        // Hang in air for duration
        yield return new WaitForSeconds(hangTime);
        isFalling = true;
    }

    private void Update()
    {
        if (isFalling && !hasLanded)
        {
            // Fall downward
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            // Check for ground collision
            if (Physics.Raycast(transform.position, Vector3.down, 0.5f))
            {
                Land();
            }
        }
    }

    private void Land()
    {
        hasLanded = true;
        isFalling = false;

        // Spawn landing effect
        if (landParticles != null)
        {
            Instantiate(landParticles, transform.position, Quaternion.identity);
        }

        // Make rock static
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isFalling || hasLanded) return;

        // Only damage if falling fast enough
        if (collision.gameObject.CompareTag("Player") &&
            GetComponent<Rigidbody>().velocity.y < -minDamageVelocity)
        {
            EntityStats playerStats = collision.gameObject.GetComponent<EntityStats>();
            if (playerStats != null)
            {
                Attack rockAttack = new Attack(
                    ownerStats.gameObject,
                    damage,
                    0f,
                    knockback,
                    1f
                );
                playerStats.TakeHit(rockAttack);
            }



            // Spawn impact effect
            if (impactParticles != null)
            {
                Instantiate(impactParticles, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);


        }
    }
}