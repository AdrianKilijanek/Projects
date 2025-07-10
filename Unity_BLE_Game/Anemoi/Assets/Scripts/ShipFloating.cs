using UnityEngine;

public class ShipFloating : MonoBehaviour
{
    public float floatStrength = 0.5f; // How much the ship moves up and down
    public float floatSpeed = 1f; // Speed of vertical movement
    public float tiltStrength = 5f; // How much the ship tilts
    public float tiltSpeed = 1.5f; // Speed of rotation

    private Vector3 startPos;
    private Quaternion startRot;
    private float randomOffset; // Random phase offset

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        randomOffset = Random.Range(0f, 2f * Mathf.PI); // Random offset for sine wave
    }

    void Update()
    {
        // Bobbing (up and down) with random offset
        float yOffset = Mathf.Sin(Time.time * floatSpeed + randomOffset) * floatStrength;
        transform.position = startPos + new Vector3(0, yOffset, 0);

        // Rocking (rotation on Z and X axes) with different random offsets
        float tiltZ = Mathf.Sin(Time.time * tiltSpeed + randomOffset) * tiltStrength;
        float tiltX = Mathf.Sin(Time.time * (tiltSpeed * 0.8f) + randomOffset * 0.5f) * tiltStrength * 0.5f;

        transform.rotation = startRot * Quaternion.Euler(tiltX, 0, tiltZ);
    }
}
