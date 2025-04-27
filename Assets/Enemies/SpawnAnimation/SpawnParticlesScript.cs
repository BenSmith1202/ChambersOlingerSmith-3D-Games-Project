using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnParticlesScript : MonoBehaviour
{
    [SerializeField] GameObject clock;

    public void SetSize(float val)
    {
        clock.GetComponent<ClockScript>().SetSize(val);
    }
}
