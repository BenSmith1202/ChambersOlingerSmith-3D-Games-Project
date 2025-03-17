using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int startHealth;
    [SerializeField] HealthBarScript healthBarScript;
    int health;

    private void Start()
    {
        health = startHealth;
        if(healthBarScript != null)
        {
            healthBarScript.SetMaxHP(startHealth);
            healthBarScript.SetHP(health);
        }
        else
        {
            print("I never got a health bar script");
        }
    }

    public void IncreaseHealth(int amount)
    {
        if(health + amount > startHealth)
        {
            health = startHealth;
        }
        else
        {
            health += amount;
        }

        if (healthBarScript != null)
        {
            healthBarScript.SetHP(health);
        }
    }

    public void DecreaseHealth(int amount)
    {
        if(health - amount < 1)
        {
            //kill gameobject
            gameObject.BroadcastMessage("Die");
        }
        else
        {
            health -= amount;
        }

        if (healthBarScript != null)
        {
            healthBarScript.SetHP(health);
        }
    }

    public int GetStartHealth()
    {
        return startHealth;
    }
}
