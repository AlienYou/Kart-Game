using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartController))]
[RequireComponent(typeof(KartLateralGrip))]
public class KartWheelVisuals : MonoBehaviour
{
    [Header("转向视觉")]
    [Tooltip("视觉轮胎转向角相对物理转向角的倍率")]
    public float steerVisualMultiplier = 1f;

    [Header("轮胎滚动")]
    [Tooltip("轮胎滚动速度倍率")]
    public float rotationSpeedMultiplier = 1f;

    private Rigidbody rb;
    private KartController controller;
    private KartLateralGrip lateralGrip;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<KartController>();
        lateralGrip = GetComponent<KartLateralGrip>();

        InitializeWheel(controller.frontLeft);
        InitializeWheel(controller.frontRight);
        InitializeWheel(controller.rearLeft);
        InitializeWheel(controller.rearRight);
    }

    private void LateUpdate()
    {
        UpdateWheel(controller.frontLeft);
        UpdateWheel(controller.frontRight);
        UpdateWheel(controller.rearLeft);
        UpdateWheel(controller.rearRight);
    }

    private void InitializeWheel(KartWheel wheel)
    {
        if (wheel == null || wheel.wheelMesh == null)
        {
            return;
        }

        wheel.wheelMeshBaseRotation = wheel.wheelMesh.localRotation;
        wheel.visualRotationAngle = 0f;
    }

    private void UpdateWheel(KartWheel wheel)
    {
        if (wheel == null || wheel.wheelVisualRoot == null || wheel.wheelMesh == null)
        {
            return;
        }

        UpdateSteeringVisual(wheel);
        UpdateRollingVisual(wheel);
    }

    private void UpdateSteeringVisual(KartWheel wheel)
    {
        float steerAngle = wheel.isFrontWheel ? lateralGrip.CurrentSteerAngle * steerVisualMultiplier : 0f;

        wheel.wheelVisualRoot.rotation = transform.rotation * Quaternion.Euler(0f, steerAngle, 0f);
    }

    private void UpdateRollingVisual(KartWheel wheel)
    {
        Vector3 pointVelocity = rb.GetPointVelocity(wheel.wheelPoint.position);
        float forwardSpeed = Vector3.Dot(pointVelocity, wheel.wheelVisualRoot.forward);

        if (wheel.radius > 0.001f)
        {
            float angularSpeed = forwardSpeed / wheel.radius;
            float angleDelta = angularSpeed * Mathf.Rad2Deg * Time.deltaTime * rotationSpeedMultiplier;
            wheel.visualRotationAngle += angleDelta;
        }

        Quaternion rollingRotation = Quaternion.AngleAxis(wheel.visualRotationAngle, wheel.rotationAxis.normalized);
        wheel.wheelMesh.localRotation = wheel.wheelMeshBaseRotation * rollingRotation;
    }
}