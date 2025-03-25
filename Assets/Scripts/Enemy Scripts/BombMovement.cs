using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombMovement : MonoBehaviour
{
    [SerializeField] float initialLift;
    [SerializeField] float lifeTime;

    EntityStats stats;

    [SerializeField] GameObject explosion;

    Rigidbody rb;
    GameObject player;

    int bounceTimeCounter = 10;
    int forceTimeCounter = 10;

    private void Start()
    {
        stats = GetComponent<EntityStats>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        StartCoroutine(Behavior());
    }

    private void Update()
    {
        if (stats.isDead) //TODO: Replace with messages or something
        {
            Die();
        }
    }

    IEnumerator Behavior()
    {
        Vector3 direction = Vector3.zero;

        //start bouncing
        while(bounceTimeCounter > 0)
        {
            rb.AddForce(new Vector3(0, stats.getJumpForce(), 0));
            bounceTimeCounter--;
            yield return new WaitForEndOfFrame();
        }

        //get desired direction
        if(player != null)
        {
            direction = (player.transform.position - rb.position).normalized;
        }
        else
        {
            direction = new Vector3(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1)).normalized;
        }

        //launch in that direction
        while (forceTimeCounter > 0)
        {
            rb.AddForce(direction * stats.getSpeed());
            forceTimeCounter--;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(lifeTime);
        Die();
    }

    public void Die()
    {
        //create explosion object
        Instantiate(explosion, transform.position, Quaternion.identity);

        //TODO: fix this to remove from pool rather than destroying
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }
}
