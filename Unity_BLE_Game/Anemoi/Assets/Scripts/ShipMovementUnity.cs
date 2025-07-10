using UnityEngine;

public class ShipMovementUnity : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of the ship's movement
    public float turnSpeed = 100f; // Speed of the ship's rotation
    private CharacterController characterController; // Reference to the CharacterController

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>(); // Get the CharacterController component attached to the ship
    }

    // Update is called once per frame
    void Update()
    {
        // Get the input for moving forward/backward (W/S or Up/Down arrows)
        float moveDirection = Input.GetAxis("Vertical"); // This will be a value between -1 and 1 (W/S or Up/Down arrows)

        // Get the input for turning left/right (A/D or Left/Right arrows)
        float turnDirection = Input.GetAxis("Horizontal"); // This will be a value between -1 and 1 (A/D or Left/Right arrows)

        // Calculate movement vector in the ship's forward direction
        Vector3 move = transform.forward * moveDirection * moveSpeed * Time.deltaTime;

        // Apply movement to the ship
        characterController.Move(move);

        // Rotate the ship based on the horizontal input (left/right turn)
        transform.Rotate(Vector3.up * turnDirection * turnSpeed * Time.deltaTime);
    }
}
