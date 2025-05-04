using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))] 
public class LightningBolt : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Effect Settings")]
    [Tooltip("How long the lightning bolt should be visible before destroying itself.")]
    public float lifetime = 0.15f; // Short duration for a burst

    [Tooltip("How many segments the line will have. More segments = smoother jaggedness but more calculation.")]
    public int segments = 15;

    [Tooltip("How 'jagged' the lightning should be. Higher values mean more deviation from a straight line.")]
    public float jaggedness = 0.2f;

    public GameObject endParts; // Optional: Assign a prefab for the end particles of the lightning bolt

    // Called once when the script instance is being loaded
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Initialize(Vector3 startPoint, Vector3 endPoint)
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LightningBolt script requires a Line Renderer component.", this);
            Destroy(gameObject); // Destroy if not set up correctly
            return;
        }

        lineRenderer.positionCount = segments;
        lineRenderer.SetPosition(0, startPoint); // Set the first point
        lineRenderer.SetPosition(segments - 1, endPoint); // Set the last point

        Vector3 direction = endPoint - startPoint;
        float totalDistance = direction.magnitude;

        // create particle effect on the surface of the object that the lightning is striking
        Vector3 particleSpawnPoint = endPoint;
        Instantiate(endParts, particleSpawnPoint, Quaternion.identity); // Instantiate end particles at the end point

        // Generate jagged intermediate points
        for (int i = 1; i < segments - 1; i++)
        {
            float progress = (float)i / (segments - 1);
            Vector3 positionOnLine = startPoint + direction * progress;

            // Calculate a random offset perpendicular to the line's main direction
            // Vector3.Cross gives a vector perpendicular to two input vectors.
            // cross the line direction with a random direction to get a random perpendicular axis.
            Vector3 perpendicularOffset = Vector3.Cross(direction, Random.onUnitSphere).normalized;

            // Apply random magnitude to the offset
            float offsetMagnitude = Random.Range(-jaggedness, jaggedness) * (totalDistance / segments);

            //Jitter more in the middle(optional visual)
            float jitterFactor = Mathf.Sin(progress * Mathf.PI); // 0 at ends, 1 in middle
            offsetMagnitude *= jitterFactor;

            lineRenderer.SetPosition(i, positionOnLine + perpendicularOffset * offsetMagnitude);
        }

       
        StartCoroutine(FadeOutAndDestroy());
    }

    //// Simple destroy after lifetime. Could be expanded to fade color/width.
    //private IEnumerator FadeOutAndDestroy()
    //{
    //    yield return new WaitForSeconds(lifetime);
    //    Destroy(gameObject);
    //}

    private IEnumerator FadeOutAndDestroy()
    {
        float timer = 0f;
        Color startColor = lineRenderer.startColor;
        Color endColor = lineRenderer.endColor;

        while (timer < lifetime)
        {
            float alpha = 1f - (timer / lifetime);
            lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            lineRenderer.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha);
            timer += Time.deltaTime;
            yield return null; // Wait for next frame
        }
        Destroy(gameObject);
    }
}