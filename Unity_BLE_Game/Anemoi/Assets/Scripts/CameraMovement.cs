using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform ship;  // Reference to the ship
    public Vector3 offset = new Vector3(0, 7, -17);  // Camera offset (height & distance behind)
    public float followSpeed = 5f;  // Speed at which the camera follows the ship
    public float rotationSpeed = 5f;  // Speed at which the camera rotates to match the ship

    void LateUpdate()
    {
        if (ship == null) return;

        // Smoothly follow the ship's position with offset
        Vector3 targetPosition = ship.position + ship.transform.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Smoothly rotate to always stay behind the ship
        Quaternion targetRotation = Quaternion.LookRotation(ship.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}