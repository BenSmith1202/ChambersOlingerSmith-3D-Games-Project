using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gateScript : MonoBehaviour
{
    // Animator for the door
    public Animator myDoor = null;

    // Array of switches attached to this door
    public SwitchScript[] switches;

    void Start()
    {
        // Ensure switches are assigned in the inspector or find them dynamically if needed
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

        // If all switches are hit, play the Open animation
        if (myDoor != null)
        {
            myDoor.SetBool("Open", true);
        }
    }
}
