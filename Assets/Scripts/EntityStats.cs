using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
    public int hp;
    public int maxHP = 100;

    public float gunRange = 100f;
    public float shootCooldownTime;
    public int clipSize;
    public float reloadTime;
    public int baseDamage; //base damage (gun deals 100% base damage, dash punch probably deals 250% base damage)
    public float baseKB; //base knockback
    public float critChance;

    public float runSpeed;     // Walking speed of the player
    public float jumpForce;     // Force applied when the player jumpss
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

    //TODO: implement universal takeHit function that takes an attack object and applies the damage, knockback,
    // and correctly modifies any relevant health bar object 
    public void TakeHit(Attack atk)
    {
        // hp - atk.damage
        // if hp <= 0, die and call
            //BuffManager attackerBuffManager = atk.owner.GetComponent<BuffManager>();
            //if (attackerBuffManager != null)
            //{
            //    attackerBuffManager.TriggerOnKillEffects(gameObject, atk);
            //}
    }


}
