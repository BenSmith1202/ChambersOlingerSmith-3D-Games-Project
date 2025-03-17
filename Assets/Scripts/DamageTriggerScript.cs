using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTriggerScript : MonoBehaviour
{
    [SerializeField] int damage = 0;

    public int GetDamage()
    { return damage; }
}
