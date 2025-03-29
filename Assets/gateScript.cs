using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gateScript : MonoBehaviour
{
    // Animator for the door
    public Animator myDoor = null;

    // Array of switches attached to this door
    public SwitchScript[] switches;

    // Audio source and clip
    private AudioSource audioSource;

    void Start()
    {
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
    }

    public void OpenDoor()
    {
        // Check if all switches are hit
        foreach (SwitchScript switchObj in switches)
        {
            if (!switchObj.isHit)
            {
                return; // Exit early if any switch is not activated
            }
        }

        // If all switches are hit, play the Open animation and sound
        if (myDoor != null)
        {
            myDoor.SetBool("Open", true);
        }

        if (audioSource != null)
        {
            audioSource.Play(); // Play the audio clip
        }
    }
}
