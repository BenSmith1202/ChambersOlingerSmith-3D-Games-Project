using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class DroneBehavior : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float desiredHeight = 5;
    [SerializeField] float awarenessDistance;
    [SerializeField] float tooCloseDistance;
    [SerializeField] GameObject deathExplosion;

    [SerializeField] AudioClip laser;
    AudioSource audSource;

    [SerializeField] GameObject droneBulletPrefab;

    EntityStats es;

    Rigidbody rb;

    BuffManager bman;


    SpawnDirector spawnDirector;

    private void Start()
    {
        if(GameObject.FindWithTag("SpawnDirector"))
        {
            spawnDirector = GameObject.FindWithTag("SpawnDirector").GetComponent<SpawnDirector>();
        }

        audSource = gameObject.GetComponent<AudioSource>();
        bman = GetComponent<BuffManager>();
        es = GetComponent<EntityStats>();
        if(player == null)
        {
            player = GameObject.Find("Player");
        }
        rb = GetComponent<Rigidbody>();
        StartCoroutine(SustainFlight());
        StartCoroutine(HorzMovement());
        StartCoroutine(TryShooting());
        StartCoroutine(FacePlayer());
    }

    private void FixedUpdate()
    {
        if (es != null && es.isDead)
        {
            Die();
        }
    }

    void Die()
    {
        Instantiate(deathExplosion, transform.position, Quaternion.identity);
        if(spawnDirector != null)
        {
            spawnDirector.RegisterKill(gameObject, 3);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Shoot(Vector3 shootDir)
    {

        // Create bullet particle

        audSource.PlayOneShot(laser);

        GameObject bulletPart = Instantiate(
            droneBulletPrefab,
            transform.position + shootDir.normalized,
            Quaternion.LookRotation(shootDir)
        );

        RaycastHit hit;
        Physics.Raycast(transform.position, shootDir, out hit, 300);
        if (hit.collider && hit.collider.gameObject.CompareTag("Player"))
        {
            hit.collider.gameObject.GetComponent<EntityStats>().InflictDamage(es.getDamage());

            //TODO: Ben has to help me understand what's going on here...
            //// make a new attack object
            //Attack bulletHit = new Attack(gameObject, es.getDamage(),
            //    es.getCritChance(), es.getKnockback(), 1); //Proc Coefficient of 1 means use default item odds.

            //bman.TriggerOnHitEffects(hit.collider.gameObject, bulletHit);

            //// apply damage
            //hit.collider.gameObject.GetComponent<EntityStats>().TakeHit(bulletHit);

            ////TODO: apply knockback
        }

        Destroy(bulletPart.gameObject, 2f);
    }

    IEnumerator FacePlayer()
    {
        while(true)
        {
            Vector3 directionToTarget = player.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z), Vector3.up);
            yield return null;
        }
    }

    IEnumerator TryShooting()
    {
        while(true)
        {
            //if the player is within attack range
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            if (distanceToPlayer < awarenessDistance)
            {
                //Warn player of shooting
                Vector3 directionToTarget = player.transform.position - transform.position;

                yield return new WaitForSeconds(0.8f);
                Shoot(directionToTarget);
            }
            yield return new WaitForSeconds(Random.Range(es.getAtkDelay(), es.getAtkDelay() + 1));
        }
    }

    IEnumerator HorzMovement()
    {
        while(true)
        {
            //if the player is within attack range
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            if (distanceToPlayer < awarenessDistance)
            {
                //if the player is visible
                //raycast for visibility
                RaycastHit hit;
                Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
                Physics.Raycast(transform.position, directionToPlayer, out hit, awarenessDistance);
                Debug.DrawRay(transform.position, directionToPlayer * awarenessDistance, Color.red, 0.1f);
                if (hit.collider && hit.collider.gameObject.CompareTag("Player"))
                {
                    //if too close
                    if(distanceToPlayer < tooCloseDistance)
                    {
                        //move away
                        rb.velocity = new Vector3(-directionToPlayer.x * es.getSpeed(), rb.velocity.y, -directionToPlayer.z * es.getSpeed());
                    }
                    //too far
                    else if(distanceToPlayer > tooCloseDistance + 1)
                    {
                        //move closer
                        rb.velocity = new Vector3(directionToPlayer.x * es.getSpeed(), rb.velocity.y, directionToPlayer.z * es.getSpeed());
                    }
                    else
                    {
                        rb.velocity = new Vector3(0, rb.velocity.y, 0);
                    }
                }
            }
            else
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
            yield return null;
        }
    }

    IEnumerator SustainFlight()
    {
        while(true)
        {
            //distance from ground
            RaycastHit hit;
            Physics.Raycast(transform.position, -transform.up, out hit, desiredHeight);

            //max distance from ground
            RaycastHit reach;
            Physics.Raycast(transform.position, -transform.up, out reach, desiredHeight + 1);

            //ceiling check
            RaycastHit hitUp;
            Physics.Raycast(transform.position, transform.up, out hitUp, 2);

            Debug.DrawRay(transform.position, transform.up * 2, Color.green, 0.1f);
            Debug.DrawRay(transform.position, -transform.up * (desiredHeight + 1), Color.green, 0.1f);

            if (hitUp.collider)
            {
                rb.velocity = new Vector3(rb.velocity.x, -es.getSpeed(), rb.velocity.z);
            }
            else if (hit.collider)
            {
                //needs to go up
                rb.velocity = new Vector3(rb.velocity.x, es.getSpeed(), rb.velocity.z);
            }
            else if(!reach.collider)
            {
                rb.velocity = new Vector3(rb.velocity.x, -es.getSpeed(), rb.velocity.z);
            }
            else
            {
                if (rb.velocity.y < 0.001f)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                }
                else
                {
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.99f, rb.velocity.z);
                }
            }
            yield return null;
        }
    }
}
