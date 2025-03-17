using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] HealthBarScript healthBarScript;
    [SerializeField] int startHealth;
    int health;

    private void Start()
    {
        health = startHealth;
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
