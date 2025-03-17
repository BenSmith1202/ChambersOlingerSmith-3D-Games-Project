using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    EntityStats stats;
    [SerializeField] HealthBarScript healthBarScript;

    private void Start()
    {
        stats = GetComponent<EntityStats>();
        stats.hp = stats.maxHP;
        if(healthBarScript != null)
        {
            healthBarScript.SetMaxHP(stats.maxHP);
            healthBarScript.SetHP(stats.hp);
        }
        else
        {
            print("I never got a health bar script");
        }
    }

    public void IncreaseHealth(int amount)
    {
        if(stats.hp + amount > stats.maxHP)
        {
            stats.hp = stats.maxHP;
        }
        else
        {
            stats.hp += amount;
        }

        if (healthBarScript != null)
        {
            healthBarScript.SetHP(stats.hp);
        }
    }

    public void DecreaseHealth(int amount)
    {
        if(stats.hp - amount < 1)
        {
            //kill gameobject
            gameObject.BroadcastMessage("Die");
        }
        else
        {
            stats.hp -= amount;
        }

        if (healthBarScript != null)
        {
            healthBarScript.SetHP(stats.hp);
        }
    }

    public int GetStartHealth()
    {
        return stats.maxHP;
    }
}
