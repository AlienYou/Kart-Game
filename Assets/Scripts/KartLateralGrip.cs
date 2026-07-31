using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartController))]
[RequireComponent(typeof(KartPhysics))]
[RequireComponent(typeof(KartInput))]
public class KartLateralGrip : MonoBehaviour
{
    [Header("抓地力")]
    public float frontGrip = 6f;
    public float rearGrip = 5.5f;
    public float maxLateralAcceleration = 30f;

    [Header("转向")]
    public float maxSteerAngle = 32f;
    public float highSpeedSteerFactor = 0.7f;
    public float steeringReferenceSpeed = 25f;
    public float steerResponse = 8f;

    [Header("低速稳定")]
    public float lowSpeedThreshold = 0.5f;
    public float lowSpeedLateralThreshold = 0.6f;
    public float lowSpeedGrip = 4f;

    [Header("调试")]
    public bool drawDebugForces = true;

    public float CurrentSteerAngle { get; private set; }

    private Rigidbody rb;
    private KartController controller;
    private KartPhysics kartPhysics;
    private KartInput input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<KartController>();
        kartPhysics = GetComponent<KartPhysics>();
        input = GetComponent<KartInput>();
    }

    private void FixedUpdate()
    {
        UpdateSteerAngle();

        ApplyWheelLateralForce(controller.frontLeft, frontGrip, CurrentSteerAngle);
        ApplyWheelLateralForce(controller.frontRight, frontGrip, CurrentSteerAngle);
        ApplyWheelLateralForce(controller.rearLeft, rearGrip, 0f);
        ApplyWheelLateralForce(controller.rearRight, rearGrip, 0f);
    }

    private void UpdateSteerAngle()
    {
        float speedRatio = Mathf.Clamp01(Mathf.Abs(kartPhysics.ForwardSpeed) / steeringReferenceSpeed);
        float steerFactor = Mathf.Lerp(1f, highSpeedSteerFactor, speedRatio);
        float targetAngle = input.Steering * maxSteerAngle * steerFactor;

        if (kartPhysics.ForwardSpeed < -0.1f)
        {
            targetAngle *= -1f;
        }

        CurrentSteerAngle = Mathf.Lerp(CurrentSteerAngle, targetAngle, steerResponse * Time.fixedDeltaTime);
    }

    private void ApplyWheelLateralForce(KartWheel wheel, float grip, float steerAngle)
    {
        if (wheel == null || wheel.wheelPoint == null || !wheel.grounded)
        {
            return;
        }

        Quaternion steerRotation = Quaternion.AngleAxis(steerAngle, transform.up);
        Vector3 wheelRight = steerRotation * transform.right;
        Vector3 wheelForward = steerRotation * transform.forward;

        Vector3 pointVelocity = rb.GetPointVelocity(wheel.wheelPoint.position);
        float lateralSpeed = Vector3.Dot(pointVelocity, wheelRight);
        float forwardSpeed = Vector3.Dot(pointVelocity, wheelForward);

        if (Mathf.Abs(forwardSpeed) < lowSpeedThreshold && Mathf.Abs(lateralSpeed) < lowSpeedLateralThreshold)
        {
            Vector3 stopForce = -wheelRight * lateralSpeed * rb.mass * lowSpeedGrip * 0.25f;
            rb.AddForceAtPosition(stopForce, wheel.wheelPoint.position, ForceMode.Force);
            return;
        }

        float lateralAcceleration = Mathf.Clamp(-lateralSpeed * grip, -maxLateralAcceleration, maxLateralAcceleration);
        Vector3 lateralForce = wheelRight * lateralAcceleration * rb.mass * 0.25f;

        rb.AddForceAtPosition(lateralForce, wheel.wheelPoint.position, ForceMode.Force);

        if (drawDebugForces)
        {
            Debug.DrawRay(wheel.wheelPoint.position, lateralForce / rb.mass * 0.1f, wheel.isFrontWheel ? Color.cyan : Color.magenta);
            Debug.DrawRay(wheel.wheelPoint.position, wheelForward * 0.5f, Color.blue);
        }
    }
}