using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockBack : MonoBehaviour
{
    [SerializeField] Collider triggerArea;
    [SerializeField] float knockBackForce;

    //float maxMagnitude;

    private void Start()
    {
        //maxMagnitude = (triggerArea.bounds.max - triggerArea.transform.position).magnitude;
        
    }

    public Vector3 GetKnockBack(Vector3 positionOfAffectedObject)
    {
        Vector3 direction = positionOfAffectedObject - triggerArea.transform.position;
        return direction * knockBackForce;
    }
}
