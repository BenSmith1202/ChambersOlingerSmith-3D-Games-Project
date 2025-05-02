using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Excalibur", menuName = "Items/OnHit/Excalibur")]
public class ExcaliburItem : ItemInstance
{
    
    public float trueDamagePercent = 0.05f; // Deals true damage equal to 5% of the target's max health
    public GameObject excaliburEffectPrefab; // Prefab for the Excalibur effect

    public override TriggerType TriggerCategory => TriggerType.OnHit;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        GameObject target = context.target;
        Attack atk = context.atk;
        GameObject myself = atk.owner;
        EntityStats targetStats = target.GetComponent<EntityStats>();
        //play sound?
        targetStats.InflictDamage(Mathf.FloorToInt(targetStats.getMaxHP() * trueDamagePercent));

        Vector3 spawnPos = target.transform.position;
        spawnPos = target.transform.position + (myself.transform.position + new Vector3(0, 1, 0) - target.transform.position).normalized;

        Instantiate(excaliburEffectPrefab, spawnPos, Quaternion.identity, target.transform);

    }

    public override void OnAcquire(EntityStats stats)
    {

    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {

    }
}
