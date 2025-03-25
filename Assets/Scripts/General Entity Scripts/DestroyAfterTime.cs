using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] float waitTime;
    private void Start()
    {
        Invoke("Die", waitTime);
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
