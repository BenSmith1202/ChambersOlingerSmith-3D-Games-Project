using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcesToRB : MonoBehaviour
{
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void KnockMeBack(Vector3 knockBack)
    {
        rb.velocity = knockBack;
        print("Launching: " + knockBack);
    }
}
