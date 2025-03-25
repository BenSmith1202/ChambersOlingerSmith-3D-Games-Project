using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtObject : MonoBehaviour
{
    [SerializeField] GameObject target;

    private void Start()
    {
        if(target == null)
        {
            target = GameObject.Find("Main Camera");
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(target.transform.forward);
    }
}
