using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableBox : MonoBehaviour
{

    //The box's current health point total
    public int currentHealth = 3;


    public static class GameEvent
    {
        public const string boxDeath = "DOOR_KEY_1";
    }



    public void Damage(int damageAmount)
    {
        //subtract damage amount when Damage function is called
        currentHealth -= damageAmount;

        //Check if health has fallen below zero
        if (currentHealth <= 0)
        {

            Messenger.Broadcast(GameEvent.boxDeath);

            //if health has fallen below zero, deactivate it (for now ill destroy to make it easier, but in the future we would use object pooling
            //gameObject.SetActive(false); 
            Destroy(gameObject);
        }
    }
}