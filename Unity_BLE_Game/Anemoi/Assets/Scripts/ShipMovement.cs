using UnityEngine;

public class ShipController : MonoBehaviour
{
    public float accelerationFactor = 5f;
    public float decelerationFactor = 2f;
    public float maxSpeed = 15f;
    public float speed = 0f;
    public float turnSpeed = 2f;

    private float currentSpeed = 0f;
    private float currentAngle = 0f;
    private float currentForce = 1f;
    private float maxHeight;

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        maxHeight = transform.position.y;
    }

    void Update()
    {
        currentSpeed = BluetoothManager.Instance.GetSpeed();
        currentAngle = BluetoothManager.Instance.GetAngle();
        currentForce = BluetoothManager.Instance.GetForce();

        if (currentSpeed > 0f)
        {
            AccelerateShip(currentSpeed);
            RotateShip(currentAngle);
        }
        else if (currentSpeed == 0f && speed > 0f)
        {
            DecelerateShip();
        }

        MoveShip();
        EnforceHeightLimit();
    }

    void AccelerateShip(float data)
    {
        speed += data * currentForce * accelerationFactor * Time.deltaTime;
        speed = Mathf.Clamp(speed, 0f, maxSpeed);
    }

    void DecelerateShip()
    {
        speed -= decelerationFactor * Time.deltaTime;
        speed = Mathf.Max(speed, 0f);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("StationaryShip"))
        {
            Debug.Log("Collided with a stationary ship!");
            Vector3 pushDirection = hit.point - transform.position;
            pushDirection.y = 0;

            characterController.Move(-pushDirection.normalized * 0.5f);
            speed = 0f;
        }
    }

    void RotateShip(float targetAngle)
    {
        float currentYRotation = transform.eulerAngles.y;
        float desiredAngle = targetAngle; // Default player input

        GameObject endPointObject = GameObject.FindGameObjectWithTag("EndPoint");

        if (BluetoothManager.Instance.GetForce() >= 2 && endPointObject != null)
        {
            Vector3 endPointPosition = endPointObject.transform.position;
            Vector3 directionToEndPoint = (endPointPosition - transform.position).normalized;

            float endpointAngle = Mathf.Atan2(directionToEndPoint.x, directionToEndPoint.z) * Mathf.Rad2Deg;
            float angleDifference = Mathf.DeltaAngle(currentYRotation, endpointAngle);

            if (Mathf.Abs(angleDifference) < 90f)
            {
                // Assist rotation only when the target is in front
                float assistFactor = Mathf.Clamp01(1f - Mathf.Abs(angleDifference) / 90f);

                float assistStrength = (currentForce == 3) ? 1.2f : 0.8f; // More aggressive assist
                desiredAngle = Mathf.LerpAngle(targetAngle, endpointAngle, assistStrength * assistFactor);
            }
        }

        // Apply the new rotation, but no straightening behavior at all
        float rotationSpeedFactor = Mathf.Lerp(0.4f, 0.8f, speed / maxSpeed);
        float newYRotation = Mathf.LerpAngle(currentYRotation, desiredAngle, rotationSpeedFactor * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, newYRotation, 0);
    }

    void MoveShip()
    {
        Vector3 movement = transform.forward * speed * Time.deltaTime;
        characterController.Move(movement);
    }

    void EnforceHeightLimit()
    {
        if (transform.position.y > maxHeight)
        {
            Vector3 correctedPosition = new Vector3(transform.position.x, maxHeight, transform.position.z);
            transform.position = correctedPosition;
        }
    }
}
