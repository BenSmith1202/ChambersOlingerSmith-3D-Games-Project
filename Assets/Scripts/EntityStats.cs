using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class EntityStats : MonoBehaviour
{
    public bool isDead;
    public int hp;
    public int maxHP = 100;

    public float baseSpeed;     // Walking speed of the entity
    public float jumpForce;     // Force applied when the entity jumpss

    public float attackCooldownTime;
    public int baseDamage; //base damage (gun deals 100% base damage, dash punch probably deals 250% base damage)
    public float critChance;
    public float range = 100f;
    public float baseKB; //base knockback

    public int clipSize;
    public float reloadTime;
    
    public float crouchSpeed;   // Speed during crouching
    public float wallrunForce;  // effectively wall run speed
    public float maxWallTime;   // Maximum time in seconds that a player can run on walls before touching the ground
    public float wallrunSpeed;   // Speed during wallrun

    public float grappleRange;
    public float grappleCooldownTime;
    public float grappleForce;  // force of the ropes tension
    public float grappleStretch;    // divides the above, how stretchy the rope is
    public float grappleReelSpeed;  // how fast the max length of the grapple decreases (linearly)
    public float grappleYankPercentDistance; // percent of player's distance from the grapple point that the rope length is initialized to
    public float swingSpeed;                 // The speed at which the player influences their momentum with the movement keys
    public float maxSwingVelocity;           // the speed cap for grappling.

    public float dashCooldownTime;
    public float dashDuration;
    public float dashSpeed;
    public float postDashSpeedReduction;
    public float minPostDashSpeed;

    public BuffManager buffManager;
    public HealthBarScript healthBarScript;
    public DamageNumbers damageNumbers;


    private void Start()
    {
        buffManager = GetComponent<BuffManager>();
        InitializeHealthBar();

        //It's ok if this is null
        damageNumbers = GetComponent<DamageNumbers>();

        SetMaxHP(maxHP);
        SetHP(maxHP);
    }


    // Universal takeHit function that takes an attack object and applies the damage, knockback,
    // and correctly modifies any relevant health bar object 
    public void TakeHit(Attack atk)
    {
        //If the attak is lethal, trigger OnKill effects
        if (atk.damage > hp)
        {
            BuffManager attackerBuffManager = atk.owner.GetComponent<BuffManager>();
            if (attackerBuffManager != null)
            {
                attackerBuffManager.TriggerOnKillEffects(gameObject, atk);
            }
        }

        InflictDamage(atk.damage);

        //TODI: apply knockback
    }


    public void InitializeHealthBar()
    {
        //check if i have a health canvas child
        Transform healthCanvas = transform.Find("HealthCanvas");
        if (healthCanvas != null)
        {
            healthBarScript = healthCanvas.gameObject.GetComponent<HealthBarScript>();
        }
        else
        {
            //Debug.Log("No enemy healthbar found, checking player");
            //check if i have a health bar myself
            // am i a player?
            PlayerHealthScript myhealth = GetComponent<PlayerHealthScript>();
            if (myhealth != null)
            {
                //if so, i can use the UI health Bar
                GameObject playerHealthBar = GameObject.FindWithTag("PlayerHealthBar");
                healthBarScript = playerHealthBar.GetComponent<HealthBarScript>();
                //Debug.Log("Player health bar found");
                // Initialize player health bar slider
                healthBarScript.healthBarSlider = playerHealthBar.GetComponent<Slider>();
            }
        }
    }


    public void SetHP(int newHP)
    {
        newHP = Mathf.Clamp(newHP, 0, maxHP); //prevents overfilling or negative HP
        hp = newHP;

        //am i dead?
        if (hp <= 0)
        {
            isDead = true;
        }

        // Trigger any on HP effects
        if (buffManager != null)
        {
            buffManager.TriggerOnHPEffects(gameObject);
        }

        //Debug.Log("Setting HP to " + newHP);

        if (healthBarScript != null)
        {
            healthBarScript.SetHP(newHP); //update healthbar
        } else
        {
            //Debug.Log("No health bar found");
        }
        
    }

    public void SetMaxHP(int newMaxHP)
    {
        //Debug.Log("Setting HP to " + newMaxHP);
        if (newMaxHP < 1) //prevents division by zero
        {
            newMaxHP = 1;
        }
        int oldMax = maxHP;
        maxHP = newMaxHP;

        if (newMaxHP < hp) //if max hp is set lower than the current hp, set the player's current hp to the new max
        {
            SetHP(newMaxHP);
        } else
        {
            Heal(newMaxHP - oldMax); // heal for the amount gained.
        }


        if (healthBarScript != null)
        {
            healthBarScript.SetMaxHP(newMaxHP); //update healthbar
        }
        else
        {
            //Debug.Log("No health bar found");
        }
        

    }

    public void IncreaseMaxHP(int amount)
    {
        SetMaxHP(maxHP + amount);
    }

    public void Heal(int amount)
    {
        SetHP(hp + amount);
        if (damageNumbers != null && damageNumbers.showDamage)
        {
            
            damageNumbers.canvasScript.ShowDamageNumber(amount);
        }
    }

    public void DecreaseMaxHP(int amount)
    {
        SetMaxHP(maxHP - amount);
    }

    public void InflictDamage(int amount)
    {
        SetHP(hp - amount);
        if (damageNumbers != null && damageNumbers.showDamage)
        {
            damageNumbers.canvasScript.ShowDamageNumber(-amount);
        }
    }
}