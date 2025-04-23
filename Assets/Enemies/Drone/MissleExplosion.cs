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
        print("Somone is uin my trugger");
        if(canDamage)
        {
            canDamage = false;
            print("can damage");
            Attack attack = new Attack(gameObject, 15, 0, 3, 0);
            if (other.GetComponent<EntityStats>())
            {
                print("dealing damage");
                other.GetComponent<EntityStats>().TakeHit(attack);
            }
        }
    }
}
