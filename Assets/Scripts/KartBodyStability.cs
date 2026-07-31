using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KartBodyStability : MonoBehaviour
{
    [Header("角速度阻尼")]
    public float pitchDamping = 2.5f;
    public float rollDamping = 3.5f;

    [Header("防翻车辅助")]
    [Tooltip("低于该角度不主动恢复，允许正常车身侧倾")]
    public float assistStartAngle = 18f;

    [Tooltip("开始介入后的直立恢复力度")]
    public float uprightStrength = 5f;

    [Tooltip("危险倾斜角")]
    public float dangerAngle = 35f;

    [Tooltip("危险状态额外恢复倍率")]
    public float dangerMultiplier = 2f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ApplyAngularDamping();
        ApplyEmergencyUprightAssist();
    }

    private void ApplyAngularDamping()
    {
        Vector3 localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);

        Vector3 localTorque = new Vector3(
            -localAngularVelocity.x * pitchDamping,
            0f,
            -localAngularVelocity.z * rollDamping
        );

        rb.AddTorque(transform.TransformDirection(localTorque), ForceMode.Acceleration);
    }

    private void ApplyEmergencyUprightAssist()
    {
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        if (tiltAngle < assistStartAngle)
        {
            return;
        }

        Vector3 correctionAxis = Vector3.Cross(transform.up, Vector3.up);

        if (correctionAxis.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float angleRange = Mathf.Max(1f, dangerAngle - assistStartAngle);
        float assistRatio = Mathf.Clamp01((tiltAngle - assistStartAngle) / angleRange);
        float strengthMultiplier = tiltAngle >= dangerAngle ? dangerMultiplier : 1f;

        Vector3 uprightTorque = correctionAxis.normalized * assistRatio * uprightStrength * strengthMultiplier;
        rb.AddTorque(uprightTorque, ForceMode.Acceleration);
    }
}