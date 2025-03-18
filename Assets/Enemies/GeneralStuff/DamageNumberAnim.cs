using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumberAnim : MonoBehaviour
{
    TMP_Text text;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
        StartCoroutine(MoveAndFade());
    }

    IEnumerator MoveAndFade()
    {
        float a = 1;
        while(a >= 0)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, a);
            text.transform.position = new Vector3(text.transform.position.x, text.transform.position.y + 0.02f, text.transform.position.z);
            a -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(gameObject);
    }
}
