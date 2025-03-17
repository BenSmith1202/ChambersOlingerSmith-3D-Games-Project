using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunModelScript : MonoBehaviour
{

    public ParticleSystem muzzleFlash;
    public ParticleSystem smoke;
    public ParticleSystem flame;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playAnimations()
    {
        muzzleFlash.Play();
        smoke.Play();
        flame.Play();
    }
}
