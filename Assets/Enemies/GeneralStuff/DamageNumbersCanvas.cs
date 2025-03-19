using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumbersCanvas : MonoBehaviour
{
    [SerializeField] GameObject damageNumberObject;

    public void ShowDamageNumber(int damageAmount)
    {
        string number = "" + damageAmount;
        if(damageAmount < 0)
        {
            //number = "-" + number;
        }
        else if(damageAmount > 0)
        {
            number = "+" + number;
        }
        spawnNumber(number);
    }

    void spawnNumber(string number)
    {
        Vector3 spawnPosition = new Vector3(
            transform.position.x + Random.Range(-0.5f, 0.5f),
            transform.position.y + Random.Range(-2f, 0),
            transform.position.z
            );
        GameObject current = Instantiate(damageNumberObject, spawnPosition, transform.rotation);
        current.transform.SetParent(gameObject.transform);
        TMP_Text text = current.GetComponent<TMP_Text>();
        text.text = number;
    }
}
