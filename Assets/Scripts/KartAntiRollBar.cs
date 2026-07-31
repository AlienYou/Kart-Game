using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartController))]
public class KartAntiRollBar : MonoBehaviour
{
    [Header("防倾杆刚度")]
    public float frontAntiRollStrength = 2500f;
    public float rearAntiRollStrength = 2000f;

    [Header("调试")]
    public bool drawDebugForces = true;

    private Rigidbody rb;
    private KartController controller;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<KartController>();
    }

    private void FixedUpdate()
    {
        ApplyAntiRoll(controller.frontLeft, controller.frontRight, frontAntiRollStrength);
        ApplyAntiRoll(controller.rearLeft, controller.rearRight, rearAntiRollStrength);
    }

    private void ApplyAntiRoll(KartWheel leftWheel, KartWheel rightWheel, float strength)
    {
        if (leftWheel == null || rightWheel == null)
        {
            return;
        }

        float leftCompression = leftWheel.grounded ? leftWheel.compression : 0f;
        float rightCompression = rightWheel.grounded ? rightWheel.compression : 0f;

        float compressionDifference = leftCompression - rightCompression;
        float antiRollForce = compressionDifference * strength;

        if (leftWheel.grounded)
        {
            Vector3 leftForce = leftWheel.wheelPoint.up * antiRollForce;
            rb.AddForceAtPosition(leftForce, leftWheel.wheelPoint.position, ForceMode.Force);

            if (drawDebugForces)
            {
                Debug.DrawRay(leftWheel.wheelPoint.position, leftForce * 0.0002f, Color.yellow);
            }
        }

        if (rightWheel.grounded)
        {
            Vector3 rightForce = -rightWheel.wheelPoint.up * antiRollForce;
            rb.AddForceAtPosition(rightForce, rightWheel.wheelPoint.position, ForceMode.Force);

            if (drawDebugForces)
            {
                Debug.DrawRay(rightWheel.wheelPoint.position, rightForce * 0.0002f, Color.yellow);
            }
        }
    }
}