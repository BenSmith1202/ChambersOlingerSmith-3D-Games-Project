using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieInSecondsScript : MonoBehaviour
{
    public float seconds = 2f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, seconds);
    }
}
