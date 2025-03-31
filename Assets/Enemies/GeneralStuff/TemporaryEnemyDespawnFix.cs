using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryEnemyDespawnFix : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float maxDistance = 100;

    private void Start()
    {
        if(player == null)
        {
            player = GameObject.Find("Player");
        }
    }

    private void Update()
    {
        if(Vector3.Distance(transform.position, player.transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
