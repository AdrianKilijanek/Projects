using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    public Transform ship;      // Assign the ship's transform
    public Transform endPoint;  // Assign the EndPoint transform

    private float fixedYPosition; // Store the initial Y position

    private void Start()
    {
        // Store the initial height of the arrow when the game starts
        fixedYPosition = transform.position.y;
    }

    private void Update()
    {
        if (ship == null || endPoint == null) return;

        // Calculate direction from ship to EndPoint
        Vector3 direction = endPoint.position - ship.position;
        direction.y = 0; // Ensure it only rotates in X-Z plane

        // Rotate the arrow to face the EndPoint
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }

        // Keep the arrow at its initial height while following the ship's X-Z position
        transform.position = new Vector3(ship.position.x, fixedYPosition, ship.position.z);
    }
}
