using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Health : MonoBehaviour
{
    EntityStats stats;
    [SerializeField] HealthBarScript healthBarScript;

    [SerializeField] GameObject dmgCanvas;
    [SerializeField] bool showDamage;

    GameObject damageNumberCanvas;
    DamageNumbersCanvas canvasScript;

    private void Start()
    {
        if(showDamage)
        {
            damageNumberCanvas = Instantiate(dmgCanvas, transform.position, transform.rotation);
            damageNumberCanvas.transform.SetParent(gameObject.transform);
            damageNumberCanvas.GetComponent<UpdatePosition>().target = gameObject;
            if(gameObject.GetComponent<Collider>())
            {
                print("new offset");
                damageNumberCanvas.GetComponent<UpdatePosition>().offset = new Vector3(0, gameObject.GetComponent<Collider>().bounds.max.y + 1, 0);
            }
            canvasScript = damageNumberCanvas.GetComponent<DamageNumbersCanvas>();
        }
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
        if(canvasScript != null)
        {
            canvasScript.ShowDamageNumber(amount);
        }

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
        if (canvasScript != null)
        {
            canvasScript.ShowDamageNumber(-amount);
        }

        if (stats.hp - amount < 1)
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
