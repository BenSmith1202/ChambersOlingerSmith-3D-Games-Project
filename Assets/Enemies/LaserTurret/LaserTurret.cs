using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] GameObject sparks;

    [SerializeField] GameObject nozzle;
    [SerializeField] float rotateSpeed;

    EntityStats es;
    LineRenderer lineRenderer;

    // Debug positions
    private Vector3 laserOrigin;
    private Vector3 laserEndPoint;

    GameObject currSparks;

    bool canDamage = true;

    private void Start()
    {
        es = GetComponent<EntityStats>();
        lineRenderer = GetComponent<LineRenderer>();

        // Ensure LineRenderer is properly configured
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
        }
        else
        {
            Debug.LogError("LineRenderer component not found!");
        }

        currSparks = Instantiate(sparks);
        currSparks.transform.SetParent(gameObject.transform);

        StartCoroutine(Rotate());
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        if (es.currentHP <= 0)
        {
            Die();
            return;
        }

        if (lineRenderer == null) return;

        // Calculate the origin point
        laserOrigin = nozzle.transform.position + nozzle.transform.rotation * new Vector3(0, 0.5f, 0);

        // Set the first point of the line renderer
        lineRenderer.SetPosition(0, laserOrigin);

        // Debug ray visualization
        Debug.DrawRay(laserOrigin, nozzle.transform.up * 100, Color.blue, 0.1f);

        RaycastHit hit;


        if (Physics.Raycast(laserOrigin, nozzle.transform.up, out hit, 200))
        {

            // We hit something
            laserEndPoint = hit.point;
            lineRenderer.SetPosition(1, laserEndPoint);

            // Draw debug line to hit point
            Debug.DrawLine(laserOrigin, laserEndPoint, Color.yellow, 0.1f);

            // Display hit normal for debugging
            Debug.DrawRay(hit.point, hit.normal, Color.green, 0.1f);

            if(hit.collider.gameObject.CompareTag("Player"))
            {
                if (canDamage)
                {
                    canDamage = false;
                    Attack attack = new Attack(gameObject, es.getDamage(), 0, 0, 0);
                    if (hit.collider.gameObject.GetComponent<EntityStats>())
                    {
                        hit.collider.gameObject.GetComponent<EntityStats>().TakeHit(attack);
                    }
                    Invoke("ResetDamage", 0.3f);
                }
            }

            currSparks.transform.position = laserEndPoint;
        }
        else
        {
            // Nothing hit, extend to maximum range
            laserEndPoint = laserOrigin + nozzle.transform.up * es.getAttackRange();

            currSparks.transform.position = laserEndPoint;
            lineRenderer.SetPosition(1, laserEndPoint);
        }
    }

    void ResetDamage()
    {
        canDamage = true;
    }

    IEnumerator Rotate()
    {
        while (true)
        {
            transform.Rotate(0, Time.deltaTime * rotateSpeed, 0);
            yield return null;
        }
    }
}