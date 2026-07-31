using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartInput))]
public class KartSteering : MonoBehaviour
{
    private Rigidbody rb;
    private KartInput input;

    [Header("转向参数")]
    [Tooltip("低速时的最大转向角")]
    public float maxSteerAngle = 32f;

    [Tooltip("高速时保留的转向比例")]
    [Range(0.1f, 1f)]
    public float highSpeedSteerFactor = 0.55f;

    [Tooltip("达到最高速转向衰减时使用的参考速度")]
    public float steeringReferenceSpeed = 25f;

    [Tooltip("车辆每秒能够旋转的速度")]
    public float yawSpeed = 90f;

    [Tooltip("倒车时是否反转转向方向")]
    public bool reverseSteeringWhenBacking = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<KartInput>();
    }

    private void FixedUpdate()
    {
        ApplySteering();
    }

    private void ApplySteering()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

        float forwardSpeed = localVelocity.z;

        // 几乎静止时不能原地转向
        if (Mathf.Abs(forwardSpeed) < 0.15f)
            return;

        float speedRatio = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / steeringReferenceSpeed);

        float steerFactor = Mathf.Lerp(1f, highSpeedSteerFactor, speedRatio);

        float steerInput = input.Steering;

        // 倒车时方向盘逻辑反向
        if (reverseSteeringWhenBacking && forwardSpeed < 0f)
        {
            steerInput *= -1f;
        }

        float targetYawSpeed = steerInput * yawSpeed * steerFactor;

        float yawDelta = targetYawSpeed * Time.fixedDeltaTime;

        Quaternion deltaRotation = Quaternion.Euler(0f, yawDelta, 0f);

        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}