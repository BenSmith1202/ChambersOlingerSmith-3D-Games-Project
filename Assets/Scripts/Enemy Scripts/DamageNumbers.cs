using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DamageNumbers : MonoBehaviour
{

    [SerializeField] GameObject dmgCanvas;
    public bool showDamage;

    GameObject damageNumberCanvas;
    public DamageNumbersCanvas canvasScript;

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
                damageNumberCanvas.GetComponent<UpdatePosition>().offset = new Vector3(0, gameObject.GetComponent<Collider>().bounds.max.y + 7, 0);
            }
            canvasScript = damageNumberCanvas.GetComponent<DamageNumbersCanvas>();
        }
    }

}
