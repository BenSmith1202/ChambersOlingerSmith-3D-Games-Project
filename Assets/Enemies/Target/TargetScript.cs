using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour
{
    [SerializeField] GameObject pop;

    public void Die()
    {
        Instantiate(pop, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
