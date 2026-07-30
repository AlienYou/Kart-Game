using System;
using UnityEngine;

[RequireComponent(typeof(KartController))]
public class KartSuspension : MonoBehaviour
{
    [Header("地面检测")]
    public LayerMask groundMask;

    [Tooltip("Raycast 起点向上的额外偏移，避免起点进入地面")]
    [Min(0f)]
    public float rayStartOffset = 0.02f;

    [Header("视觉")]
    public bool updateWheelVisuals = true;

    [Header("调试")]
    public bool drawDebugRays = true;

    KartController controller;

    void Awake()
    {
        controller = GetComponent<KartController>();
    }

    void FixedUpdate()
    {
        UpdateWheel(controller.frontLeft);
        UpdateWheel(controller.frontRight);
        UpdateWheel(controller.rearLeft);
        UpdateWheel(controller.rearRight);
    }

    void UpdateWheel(KartWheel wheel)
    {
        if (wheel == null || wheel.wheelPoint == null)
        {
            return;
        }
        Vector3 suspensionUp = wheel.wheelPoint.up;
        Vector3 suspensionDown = -suspensionUp;

        Vector3 rayOrigin = wheel.wheelPoint.position + suspensionUp * rayStartOffset;
        float rayLength = rayStartOffset + wheel.suspensionLength + wheel.radius;
        bool hasHit = Physics.Raycast(rayOrigin, suspensionDown, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore);
        wheel.grounded = hasHit;
        if (hasHit)
        {
            wheel.hit = hit;

            /*
             * hit.distance 是：
             * Raycast 起点到地面接触点的距离。
             *
             * 减去 rayStartOffset：
             * 得到 WheelPoint 到地面的距离。
             *
             * 再减去轮胎半径：
             * 得到 WheelPoint 到轮胎中心的距离。
             */
            float suspensionDistance = hit.distance - rayStartOffset - wheel.radius;

            wheel.currentSuspensionLength = Mathf.Clamp(suspensionDistance, 0f, wheel.suspensionLength);

            wheel.compression = 1f - wheel.currentSuspensionLength / wheel.suspensionLength;

            if (updateWheelVisuals)
            {
                UpdateGroundedWheelVisual(wheel, suspensionUp);
            }
        }
        else
        {
            wheel.currentSuspensionLength = wheel.suspensionLength;

            wheel.compression = 0f;

            if (updateWheelVisuals)
            {
                UpdateAirborneWheelVisual(wheel, suspensionUp);
            }
        }

        if (drawDebugRays)
        {
            DrawWheelRay(rayOrigin, suspensionDown, rayLength, hasHit, hit);
        }
    }

    private void UpdateGroundedWheelVisual(KartWheel wheel, Vector3 suspensionUp)
    {
        if (wheel.wheelMesh == null)
            return;

        /*
         * 轮胎中心位于地面接触点上方一个轮胎半径。
         * 使用地面法线比直接使用车辆 up 更适合斜坡。
         */
        wheel.wheelMesh.position = wheel.hit.point + wheel.hit.normal * wheel.radius;
    }

    private void UpdateAirborneWheelVisual(KartWheel wheel, Vector3 suspensionUp)
    {
        if (wheel.wheelMesh == null)
        {
            return;
        }
        wheel.wheelMesh.position = wheel.wheelPoint.position - suspensionUp * wheel.suspensionLength;
    }

    private void DrawWheelRay(Vector3 origin, Vector3 direction, float length, bool hasHit, RaycastHit hit)
    {
        if (hasHit)
        {
            Debug.DrawLine(origin, hit.point, Color.green);
            Debug.DrawLine(hit.point, origin + direction * length, Color.yellow);
        }
        else
        {
            Debug.DrawRay(origin, direction * length, Color.red);
        }
    }

    void DrawWheelPoint(KartWheel wheel)
    {
        Gizmos.DrawSphere(wheel.wheelPoint.position, 0.05f);
    }

    void OnDrawGizmos()
    {
        if (controller == null)
            return;
        DrawWheelPoint(controller.frontLeft);
        DrawWheelPoint(controller.frontRight);
        DrawWheelPoint(controller.rearLeft);
        DrawWheelPoint(controller.rearRight);
    }

    private void OnGUI()
    {
        if (controller == null)
            return;

        GUI.Label(new Rect(20, 90, 400, 25), $"FL Grounded: {controller.frontLeft.grounded}  " + $"Compression: {controller.frontLeft.compression:F2}");
        GUI.Label(new Rect(20, 115, 400, 25), $"FR Grounded: {controller.frontRight.grounded}  " + $"Compression: {controller.frontRight.compression:F2}");
        GUI.Label(new Rect(20, 140, 400, 25), $"RL Grounded: {controller.rearLeft.grounded}  " + $"Compression: {controller.rearLeft.compression:F2}");
        GUI.Label(new Rect(20, 165, 400, 25), $"RR Grounded: {controller.rearRight.grounded}  " + $"Compression: {controller.rearRight.compression:F2}");
    }
}
