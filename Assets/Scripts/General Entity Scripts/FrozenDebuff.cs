using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//A basic and easy to copy debuff effect.
public class FrozenDebuff : BuffInstance
{
    public AudioClip inflictSound;
    public float soundVolume = 1f;

    Rigidbody rb;

    private void Start()
    {

        GameObject parent = transform.parent.gameObject;
        bmanager = parent.GetComponent<BuffManager>();
        rb = parent.GetComponent<Rigidbody>();
        if (bmanager == null)
        {
            //if not attatched to anything, buff shouldnt exist
            Debug.LogWarning("There is an Orphaned Buff!");
            Destroy(gameObject);
        }
        stats = parent.GetComponent<EntityStats>();
        if (stats == null)
        {
            OnRemove();
        }
    }

    public override void OnApply()
    {
        AudioSource.PlayClipAtPoint(inflictSound, transform.position, soundVolume);

        if (rb != null) 
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic= true;
        }

        
        // Ow im frozen. play a sound effect
    }


    public override void OnTick()
    {


    }


    public override void OnRemove()
    {
        //DESTROY! IMPORTANT!
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        Destroy(gameObject);
    }

}
