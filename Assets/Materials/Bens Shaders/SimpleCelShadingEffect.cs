using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SimpleCelShadingEffect : MonoBehaviour
{
    [Header("Shader Settings")]
    public Shader celShader;

    [Header("Color Banding")]
    [Range(2, 128)]
    public int colorLevels = 4;

    [Header("Outline")]
    public bool enableOutline = true;
    [Range(0, 5)]
    public float outlineThickness = 1.0f;
    public Color outlineColor = Color.black;
    [Range(0.01f, 0.5f)]
    public float edgeThreshold = 0.05f;

    private Material celMaterial;
    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.DepthNormals;

        if (celShader != null && celMaterial == null)
        {
            celMaterial = new Material(celShader);
            celMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnDisable()
    {
        if (celMaterial != null)
        {
            DestroyImmediate(celMaterial);
            celMaterial = null;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (celShader != null && celMaterial != null)
        {
            celMaterial.SetFloat("_ColorLevels", colorLevels);
            celMaterial.SetFloat("_EnableOutline", enableOutline ? 1.0f : 0.0f);
            celMaterial.SetFloat("_OutlineThickness", outlineThickness);
            celMaterial.SetColor("_OutlineColor", outlineColor);
            celMaterial.SetFloat("_EdgeThreshold", edgeThreshold);

            Graphics.Blit(source, destination, celMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}