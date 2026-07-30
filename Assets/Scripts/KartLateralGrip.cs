using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartController))]
[RequireComponent(typeof(KartPhysics))]
public class KartLateralGrip : MonoBehaviour
{
    [Header("前后轮抓地力")]
    [Tooltip("前轮横向抓地系数")]
    public float frontGrip = 6f;

    [Tooltip("后轮横向抓地系数")]
    public float rearGrip = 7f;

    [Header("抓地力限制")]
    [Tooltip("单个轮胎允许施加的最大侧向加速度")]
    public float maxLateralAcceleration = 25f;

    [Header("低速处理")]
    [Tooltip("低于该速度时降低横向力，避免停车时抖动")]
    public float minimumGripSpeed = 0.5f;

    [Header("调试")]
    public bool drawDebugForces = true;

    Rigidbody rb;
    KartController controller;
    KartPhysics kartPhysics;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<KartController>();
        kartPhysics = GetComponent<KartPhysics>();
    }

    void FixedUpdate()
    {
        ApplyWheelLateralForce(controller.frontLeft, frontGrip);
        ApplyWheelLateralForce(controller.frontRight, frontGrip);
        ApplyWheelLateralForce(controller.rearLeft, rearGrip);
        ApplyWheelLateralForce(controller.rearRight, rearGrip);
    }

    void ApplyWheelLateralForce(KartWheel wheel, float grip)
    {
        if (wheel == null || wheel.wheelPoint == null)
        {
            return;
        }

        Vector3 pointVelocity = rb.GetPointVelocity(wheel.wheelPoint.position);
        Vector3 localPointVelocity = transform.InverseTransformDirection(pointVelocity);
        float lateralSpeed = localPointVelocity.x;
        if (Mathf.Abs(localPointVelocity.z) < minimumGripSpeed && Mathf.Abs(lateralSpeed) < minimumGripSpeed)
        {
            return;
        }

        float lateralAcceleration = -lateralSpeed * grip;
        lateralAcceleration = Mathf.Clamp(lateralAcceleration, -maxLateralAcceleration, maxLateralAcceleration);

        Vector3 lateralForce = transform.right * lateralAcceleration * rb.mass * 0.25f;
        rb.AddForceAtPosition(lateralForce, wheel.wheelPoint.position);

        if (drawDebugForces)
        {
            Debug.DrawRay(wheel.wheelPoint.position, lateralForce / rb.mass * 0.1f, Color.cyan);
        }
    }
}
