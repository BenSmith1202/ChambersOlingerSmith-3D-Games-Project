using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MechScript : MonoBehaviour
{
    GameObject player;
    Rigidbody rb;

    [SerializeField] float visibilityDistance;
    [SerializeField] int rotateSpeed = 30;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        StartCoroutine(Behavior());
    }

    IEnumerator Behavior()
    {
        while(true)
        {
            if(GetDistanceToPlayer() < visibilityDistance)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, GetRotationToPlayer(), rotateSpeed * Time.deltaTime);
            }
            else
            {

            }
            yield return null;
        }
    }

    float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.transform.position);
    }

    Quaternion GetRotationToPlayer()
    {
        return Quaternion.LookRotation(GetXZDirectionToPlayer(), Vector3.up);
    }

    Vector3 GetXZDirectionToPlayer()
    {
        Vector3 dir = (transform.position - player.transform.position).normalized;

        return new Vector3(dir.x, 0, dir.z);
    }
}
