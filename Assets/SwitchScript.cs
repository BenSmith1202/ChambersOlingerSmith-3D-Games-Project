using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScript : MonoBehaviour
{
    public bool isHit = false; // Track if the switch is activated

    private gateScript linkedDoor; // Reference to the door this switch controls

    public GameObject door;
    public EntityStats stats;
    public Animator leverAnim;

    // Audio source
    private AudioSource audioSource;

    private void Start()
    {
        linkedDoor = door.GetComponent<gateScript>();
        stats = gameObject.GetComponent<EntityStats>();
        leverAnim = GetComponent<Animator>();

        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
    }

    // Call this method when the switch is activated
    public void ActivateSwitch()
    {
        if (!isHit)
        {
            isHit = true;
            linkedDoor.OpenDoor(); // Check if all switches are hit

            // Play sound effect
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }

    private void Update()
    {
        if (stats.currentHP <= 0)
        {
            ActivateSwitch();
            leverAnim.SetBool("isHit", true);
        }
    }
}
