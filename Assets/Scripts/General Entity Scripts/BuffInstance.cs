using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BuffInstance : MonoBehaviour
{
    public string buffName;
    public Image icon;
    public float maxDuration = 5f;
    public float currentDuration = 0f;
    public float maxTickDelay = 1f;
    public float currentTickDelay = 0f;
    protected BuffManager bmanager;
    protected EntityStats stats;

    //this method only really exists for copy-paste purposes
    private void Start()
    {
        GameObject parent = transform.parent.gameObject;
        bmanager = parent.GetComponent<BuffManager>();
        if (bmanager == null)
        {
            //if not attatched to anything, buff shouldnt exist
            Debug.LogWarning("Orphaned Buff!");
            Destroy(gameObject);
        }
        stats = bmanager.myStats;

    }

    public abstract void OnApply();
    public abstract void OnTick();
    public abstract void OnRemove();

}
