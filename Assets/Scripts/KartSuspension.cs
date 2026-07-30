using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartController))]
public class KartSuspension : MonoBehaviour
{
    [Header("地面检测")]
    public LayerMask groundMask;

    [Tooltip("Raycast 起点向上的偏移")]
    [Min(0f)]
    public float rayStartOffset = 0.02f;

    [Header("悬挂参数")]
    [Tooltip("单个轮子的弹簧刚度")]
    [Min(0f)]
    public float springStrength = 12000f;

    [Tooltip("单个轮子的减震系数")]
    [Min(0f)]
    public float damperStrength = 1800f;

    [Tooltip("单个轮子允许施加的最大悬挂力")]
    [Min(0f)]
    public float maxSuspensionForce = 12000f;

    [Header("视觉")]
    public bool updateWheelVisuals = true;

    [Header("调试")]
    public bool drawDebugRays = true;
    public bool drawDebugForces = true;

    private Rigidbody rb;
    private KartController controller;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<KartController>();

        InitializeWheel(controller.frontLeft);
        InitializeWheel(controller.frontRight);
        InitializeWheel(controller.rearLeft);
        InitializeWheel(controller.rearRight);
    }

    private void FixedUpdate()
    {
        UpdateWheel(controller.frontLeft);
        UpdateWheel(controller.frontRight);
        UpdateWheel(controller.rearLeft);
        UpdateWheel(controller.rearRight);
    }

    private void InitializeWheel(KartWheel wheel)
    {
        if (wheel == null)
            return;

        wheel.currentSuspensionLength = wheel.suspensionLength;
        wheel.previousSuspensionLength = wheel.suspensionLength;
        wheel.suspensionVelocity = 0f;
        wheel.suspensionForce = 0f;
    }

    private void UpdateWheel(KartWheel wheel)
    {
        if (wheel == null || wheel.wheelPoint == null)
            return;

        Vector3 suspensionUp = wheel.wheelPoint.up;
        Vector3 suspensionDown = -suspensionUp;

        Vector3 rayOrigin = wheel.wheelPoint.position + suspensionUp * rayStartOffset;

        float rayLength = rayStartOffset + wheel.suspensionLength + wheel.radius;

        bool hasHit = Physics.Raycast(rayOrigin, suspensionDown, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore);

        wheel.grounded = hasHit;

        if (hasHit)
        {
            wheel.hit = hit;

            float suspensionDistance = hit.distance - rayStartOffset - wheel.radius;

            wheel.currentSuspensionLength = Mathf.Clamp(suspensionDistance, 0f, wheel.suspensionLength);

            float compressionDistance = wheel.suspensionLength - wheel.currentSuspensionLength;

            wheel.compression = compressionDistance / wheel.suspensionLength;

            CalculateAndApplySuspensionForce(wheel, suspensionUp, compressionDistance);

            if (updateWheelVisuals)
            {
                UpdateGroundedWheelVisual(wheel);
            }
        }
        else
        {
            wheel.currentSuspensionLength = wheel.suspensionLength;

            wheel.compression = 0f;
            wheel.suspensionVelocity = 0f;
            wheel.suspensionForce = 0f;

            if (updateWheelVisuals)
            {
                UpdateAirborneWheelVisual(wheel, suspensionUp);
            }
        }

        wheel.previousSuspensionLength = wheel.currentSuspensionLength;

        if (drawDebugRays)
        {
            DrawWheelRay(rayOrigin, suspensionDown, rayLength, hasHit, hit);
        }
    }

    private void CalculateAndApplySuspensionForce(KartWheel wheel, Vector3 suspensionUp, float compressionDistance)
    {
        wheel.suspensionVelocity = (wheel.previousSuspensionLength - wheel.currentSuspensionLength) / Time.fixedDeltaTime;

        float springForce = compressionDistance * springStrength;

        float damperForce = wheel.suspensionVelocity * damperStrength;

        float totalForce = springForce + damperForce;

        /*
         * 悬挂只能推动车身，不能把车身向下吸。
         */
        totalForce = Mathf.Clamp(totalForce, 0f, maxSuspensionForce);

        wheel.suspensionForce = totalForce;

        Vector3 force = suspensionUp * totalForce;

        rb.AddForceAtPosition(force, wheel.wheelPoint.position, ForceMode.Force);

        if (drawDebugForces)
        {
            Debug.DrawRay(wheel.wheelPoint.position, suspensionUp * totalForce * 0.0001f, Color.blue);
        }
    }

    private void UpdateGroundedWheelVisual(KartWheel wheel)
    {
        if (wheel.wheelMesh == null)
            return;

        wheel.wheelMesh.position = wheel.hit.point + wheel.hit.normal * wheel.radius;
    }

    private void UpdateAirborneWheelVisual(KartWheel wheel, Vector3 suspensionUp)
    {
        if (wheel.wheelMesh == null)
            return;

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
    private void OnGUI()
    {
        if (controller == null)
            return;

        GUI.Label(
            new Rect(20, 90, 400, 25),
            $"FL Grounded: {controller.frontLeft.grounded}  " +
            $"Compression: {controller.frontLeft.compression:F2}"
        );

        GUI.Label(
            new Rect(20, 115, 400, 25),
            $"FR Grounded: {controller.frontRight.grounded}  " +
            $"Compression: {controller.frontRight.compression:F2}"
        );

        GUI.Label(
            new Rect(20, 140, 400, 25),
            $"RL Grounded: {controller.rearLeft.grounded}  " +
            $"Compression: {controller.rearLeft.compression:F2}"
        );

        GUI.Label(
            new Rect(20, 165, 400, 25),
            $"RR Grounded: {controller.rearRight.grounded}  " +
            $"Compression: {controller.rearRight.compression:F2}"
        );
    }
}