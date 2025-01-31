using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthScript : MonoBehaviour
{

    public int hp;
    public int maxHP = 100;
    public GameObject healthBar;
    HealthBarScript healthBarScript;
    // Start is called before the first frame update
    void Start()
    {
        healthBarScript = healthBar.GetComponent<HealthBarScript>();
        SetPlayerHP(maxHP);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            SetPlayerHP(hp - 5);
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            SetPlayerHP(hp + 5);
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            SetPlayerMaxHP(maxHP - 5);
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            SetPlayerMaxHP(maxHP + 5);
        }
    }

    public void SetPlayerHP(int newHP)
    {
        newHP = Mathf.Clamp(newHP, 0, maxHP); //prevents overfilling or negative HP

        hp = newHP;
        healthBarScript.SetHP(newHP); //update healthbar
    }

    public void SetPlayerMaxHP(int newMaxHP)
    {
        if (newMaxHP < 1) //prevents division by zero
        {
            newMaxHP = 1;
        }

        maxHP = newMaxHP;

        if (newMaxHP < hp) //if max hp is set lower than the current hp, set the player's current hp to the new max
        {
            SetPlayerHP(newMaxHP);
        }

        healthBarScript.SetMaxHP(newMaxHP); //update healthbar
    }

}
