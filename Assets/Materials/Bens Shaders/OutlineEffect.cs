using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class OutlineEffect : MonoBehaviour
{
    [Header("Shader Settings")]
    public Shader outlineShader;

    [Header("Outline Parameters")]
    [Range(0, 5)]
    public float outlineThickness = 1.0f;
    public Color outlineColor = Color.black;
    [Range(0.01f, 0.5f)]
    public float depthThreshold = 0.05f;
    [Range(0.1f, 1.0f)]
    public float normalThreshold = 0.5f;

    private Material outlineMaterial;
    private Camera cam;

    void OnEnable()
    {
        // Make sure we have a camera and it has depth texture mode enabled
        cam = GetComponent<Camera>();
        cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.DepthNormals;

        // Create material if needed
        if (outlineShader != null && outlineMaterial == null)
        {
            outlineMaterial = new Material(outlineShader);
            outlineMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnDisable()
    {
        if (outlineMaterial != null)
        {
            DestroyImmediate(outlineMaterial);
            outlineMaterial = null;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (outlineShader != null && outlineMaterial != null)
        {
            // Update material properties
            outlineMaterial.SetFloat("_OutlineThickness", outlineThickness);
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_DepthThreshold", depthThreshold);
            outlineMaterial.SetFloat("_NormalThreshold", normalThreshold);

            Graphics.Blit(source, destination, outlineMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}