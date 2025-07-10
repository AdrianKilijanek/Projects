using UnityEngine;

public class TreasureFloating : MonoBehaviour
{
    public float floatStrength = 1f; // Stronger bobbing motion
    public float floatSpeed = 0.5f; // Slower movement
    public float tiltStrength = 10f; // More dramatic tilting
    public float tiltSpeed = 0.8f; // Slower tilting speed

    public Light glowLight; // Optional light component for extra shine
    public Renderer treasureRenderer; // Renderer for glowing effect

    private Vector3 startPos;
    private Quaternion startRot;
    private float randomOffset;
    private Material treasureMaterial; // Reference to material
    private Transform parentTransform;

    void Start()
    {
        parentTransform = transform.parent; // Store parent reference

        if (parentTransform != null)
        {
            startPos = parentTransform.position + transform.localPosition;
        }
        else
        {
            startPos = transform.position;
        }
        
        startRot = transform.rotation;
        randomOffset = Random.Range(0f, 2f * Mathf.PI); // Random phase offset

        // Auto-find components if not manually assigned
        if (glowLight == null) glowLight = GetComponentInChildren<Light>();
        if (treasureRenderer == null) treasureRenderer = GetComponent<Renderer>();

        // Get and modify the material for emission
        if (treasureRenderer != null)
        {
            treasureMaterial = treasureRenderer.material; // Get the material instance
            treasureMaterial.EnableKeyword("_EMISSION");  // Ensure emission is enabled
        }
    }

    void Update()
    {
        // Update start position dynamically if the parent has moved
        if (parentTransform != null)
        {
            startPos = parentTransform.position + transform.localPosition;
        }

        // Heavy bobbing motion
        float yOffset = Mathf.Sin(Time.time * floatSpeed + randomOffset) * floatStrength;
        transform.position = startPos + new Vector3(0, yOffset, 0);

        // Stronger rocking motion
        float tiltZ = Mathf.Sin(Time.time * tiltSpeed + randomOffset) * tiltStrength;
        float tiltX = Mathf.Sin(Time.time * (tiltSpeed * 0.6f) + randomOffset) * tiltStrength * 0.5f;

        transform.rotation = startRot * Quaternion.Euler(tiltX, 0, tiltZ);

        // Shining effect (pulsing light intensity)
        if (glowLight != null)
        {
            glowLight.intensity = 2f + Mathf.Sin(Time.time * 2f) * 1f; // Pulses between 1 and 3
        }

        // Automatic Material Emission Glow
        if (treasureMaterial != null)
        {
            float glow = 0.5f + Mathf.Sin(Time.time * 2f) * 0.5f; // Pulses between 0 and 1
            Color emissionColor = new Color(glow, glow * 0.8f, 0) * 2f; // Golden color
            treasureMaterial.SetColor("_EmissionColor", emissionColor);
        }
    }
}
