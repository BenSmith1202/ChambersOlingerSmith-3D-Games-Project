using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//A basic and easy to copy debuff effect.
public class BurningDebuff : BuffInstance
{
    public AudioClip inflictSound;
    public float soundVolume = 1f;
    public int damage = 1;

    private void Start()
    {
        GameObject parent = transform.parent.gameObject;
        bmanager = parent.GetComponent<BuffManager>();
        if (bmanager == null)
        {
            //if not attatched to anything, buff shouldnt exist
            Debug.LogWarning("There is an Orphaned Buff!");
            Destroy(gameObject);
        }
        stats = bmanager.stats;
    }

    public override void OnApply()
    {
        AudioSource.PlayClipAtPoint(inflictSound, transform.position, soundVolume);
        // Ow im burning. play a sound effect
    }
    public override void OnTick()
    {
        if (stats != null)
        {
            stats.InflictDamage(damage);
        }
        
    }
    public override void OnRemove()
    {
        //DESTROY! IMPORTANT!
        Destroy(gameObject);
    }

}
