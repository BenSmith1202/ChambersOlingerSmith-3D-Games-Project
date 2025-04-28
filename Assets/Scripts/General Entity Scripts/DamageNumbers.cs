using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DamageNumbers : MonoBehaviour
{
    [SerializeField] Vector3 addedOffset;

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
                float tallPoint;
                if(gameObject.GetComponent<MeshFilter>())
                {
                    tallPoint = gameObject.GetComponent<MeshFilter>().mesh.bounds.size.y * gameObject.transform.localScale.y;
                }
                else if(gameObject.GetComponent<Collider>())
                {
                    tallPoint = gameObject.GetComponent<Collider>().bounds.size.y * 2;
                }
                else
                {
                    tallPoint = gameObject.transform.localScale.y + 5;
                }

                //print("Offset: " + tallPoint);
                damageNumberCanvas.GetComponent<UpdatePosition>().offset = new Vector3(0, tallPoint * 1.5f, 0) + addedOffset;
            }
            else
            {
                print("no collider");
            }
            canvasScript = damageNumberCanvas.GetComponent<DamageNumbersCanvas>();
        }
    }

}
