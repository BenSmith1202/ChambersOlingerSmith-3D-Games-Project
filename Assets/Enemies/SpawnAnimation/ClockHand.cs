using UnityEngine;

public class ClockHand : MonoBehaviour
{
    [SerializeField] private float handLength = 1.0f;
    [SerializeField] private float rotationSpeed = 30.0f; // Degrees per second

    private LineRenderer lineRenderer;

    void Start()
    {
        // Get or add LineRenderer component
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 2;

        // Set the line points
        lineRenderer.SetPosition(0, Vector3.zero); // Origin point
        lineRenderer.SetPosition(1, new Vector3(0, 0, handLength)); // End point (initially pointing up)
    }

    void Update()
    {
        // Rotate the object (and thus the LineRenderer)
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}