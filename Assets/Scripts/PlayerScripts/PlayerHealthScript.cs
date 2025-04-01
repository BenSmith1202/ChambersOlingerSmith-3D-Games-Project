using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealthScript : MonoBehaviour
{

    EntityStats stats;
    public GameObject healthBar;

    ForcesToRB forcesToRBScript;
    public HealthBarScript healthBarScript;
    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<EntityStats>();
        forcesToRBScript = GetComponent<ForcesToRB>();
        healthBarScript = healthBar.GetComponent<HealthBarScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (stats.currentHP <= 0)
        {
            SceneManager.LoadScene("DeathScreen");
        }

        //DEBUG TOOLS
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            stats.InflictDamage(5);
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            stats.Heal(5);
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            stats.DecreaseMaxHP(5);
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            stats.IncreaseMaxHP(5);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<DamageTriggerScript>())
        {
            stats.InflictDamage(other.gameObject.GetComponent<DamageTriggerScript>().GetDamage());
        }
        //if(other.gameObject.GetComponent<KnockBack>())
        //{
        //    forcesToRBScript.KnockMeBack(other.gameObject.GetComponent<KnockBack>().GetKnockBack(transform.position));
        //}
    }

}
