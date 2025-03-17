using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthScript : MonoBehaviour
{

    EntityStats stats;
    public GameObject healthBar;

    ForcesToRB forcesToRBScript;
    HealthBarScript healthBarScript;
    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<EntityStats>();
        forcesToRBScript = GetComponent<ForcesToRB>();
        healthBarScript = healthBar.GetComponent<HealthBarScript>();
        SetPlayerHP(stats.maxHP);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            SetPlayerHP(stats.hp - 5);
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            SetPlayerHP(stats.hp + 5);
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            SetPlayerMaxHP(stats.maxHP - 5);
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            SetPlayerMaxHP(stats.maxHP + 5);
        }
    }

    public void SetPlayerHP(int newHP)
    {
        newHP = Mathf.Clamp(newHP, 0, stats.maxHP); //prevents overfilling or negative HP

        stats.hp = newHP;
        healthBarScript.SetHP(newHP); //update healthbar
    }

    public void SetPlayerMaxHP(int newMaxHP)
    {
        if (newMaxHP < 1) //prevents division by zero
        {
            newMaxHP = 1;
        }

        stats.maxHP = newMaxHP;

        if (newMaxHP < stats.hp) //if max hp is set lower than the current hp, set the player's current hp to the new max
        {
            SetPlayerHP(newMaxHP);
        }

        healthBarScript.SetMaxHP(newMaxHP); //update healthbar
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<DamageTriggerScript>())
        {
            SetPlayerHP(stats.hp - other.gameObject.GetComponent<DamageTriggerScript>().GetDamage());
        }
        if(other.gameObject.GetComponent<KnockBack>())
        {
            forcesToRBScript.KnockMeBack(other.gameObject.GetComponent<KnockBack>().GetKnockBack(transform.position));
        }
    }

}
