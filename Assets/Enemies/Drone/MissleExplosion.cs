using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleExplosion : MonoBehaviour
{
    bool canDamage = true;

    private void Start()
    {
        Invoke("CantDamage", 0.1f);
    }

    void CantDamage()
    {
        canDamage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(canDamage)
        {
            canDamage = false;
            Attack attack = new Attack(gameObject, 15, 0, 3, 0);
            if (other.GetComponent<EntityStats>())
            {
                other.GetComponent<EntityStats>().TakeHit(attack);
            }
        }
    }
}
