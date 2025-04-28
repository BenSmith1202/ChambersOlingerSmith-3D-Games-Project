using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockScript : MonoBehaviour
{
    [SerializeField] GameObject tick;
    [SerializeField] float distanceToCenter = 1;
    [SerializeField] int numberOfTicks = 12;

    Vector3 offset;

    private void Start()
    {
        offset = new Vector3(0, 0, distanceToCenter);
        SpawnTicks();
    }

    void SpawnTicks()
    {
        float rotationDeg = 360 / numberOfTicks;

        for(int i = 0; i < numberOfTicks; i++)
        {
            Quaternion rot = Quaternion.Euler(0, rotationDeg * i, 0);
            Vector3 pos = transform.position + rot * offset;
            GameObject t = Instantiate(tick, pos, rot);
            t.transform.SetParent(transform);
            if((i * rotationDeg) % 90 == 0)
            {
                t.transform.localScale = t.transform.localScale * 1.5f;
            }
        }
    }

    public void SetSize(float val)
    {
        transform.localScale = transform.localScale * val;
        distanceToCenter = val;
    }
}
