using UnityEngine;

[DefaultExecutionOrder(-150)]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartInput))]
[RequireComponent(typeof(KartPhysics))]
[RequireComponent(typeof(KartLateralGrip))]
public class KartDrift : MonoBehaviour
{
    [Header("漂移触发")]
    public float minimumDriftSpeed = 5f;
    public float minimumSteeringInput = 0.25f;

    [Header("后轮抓地")]
    [Range(0.1f, 1f)]
    public float driftingRearGripMultiplier = 0.45f;

    public float gripEnterSpeed = 8f;
    public float gripExitSpeed = 5f;

    [Header("漂移辅助")]
    public float yawAssistAcceleration = 5f;
    public float maxYawAssistAcceleration = 8f;

    [Header("状态")]
    public bool showDebugInfo = true;

    public bool IsDrifting { get; private set; }
    public float RearGripMultiplier { get; private set; } = 1f;

    private Rigidbody rb;
    private KartInput input;
    private KartPhysics kartPhysics;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<KartInput>();
        kartPhysics = GetComponent<KartPhysics>();
    }

    private void FixedUpdate()
    {
        UpdateDriftState();
        UpdateRearGrip();
        ApplyDriftYawAssist();
    }

    private void UpdateDriftState()
    {
        float speed = Mathf.Abs(kartPhysics.ForwardSpeed);
        float steeringAmount = Mathf.Abs(input.Steering);

        bool canStartDrift =
            input.Drift &&
            speed >= minimumDriftSpeed &&
            steeringAmount >= minimumSteeringInput;

        if (!IsDrifting)
        {
            if (canStartDrift)
            {
                IsDrifting = true;
            }

            return;
        }

        if (!input.Drift || speed < minimumDriftSpeed * 0.6f)
        {
            IsDrifting = false;
        }
    }

    private void UpdateRearGrip()
    {
        float targetMultiplier = IsDrifting ? driftingRearGripMultiplier : 1f;
        float responseSpeed = IsDrifting ? gripEnterSpeed : gripExitSpeed;

        RearGripMultiplier = Mathf.MoveTowards(
            RearGripMultiplier,
            targetMultiplier,
            responseSpeed * Time.fixedDeltaTime
        );
    }

    private void ApplyDriftYawAssist()
    {
        if (!IsDrifting)
        {
            return;
        }

        float speedRatio = Mathf.Clamp01(
            Mathf.Abs(kartPhysics.ForwardSpeed) / 15f
        );

        float yawAcceleration =
            input.Steering *
            yawAssistAcceleration *
            speedRatio;

        yawAcceleration = Mathf.Clamp(
            yawAcceleration,
            -maxYawAssistAcceleration,
            maxYawAssistAcceleration
        );

        rb.AddTorque(
            transform.up * yawAcceleration,
            ForceMode.Acceleration
        );
    }

    private void OnGUI()
    {
        if (!showDebugInfo)
        {
            return;
        }

        GUI.Label(
            new Rect(20, 380, 500, 25),
            $"Drifting: {IsDrifting}"
        );

        GUI.Label(
            new Rect(20, 405, 500, 25),
            $"Rear Grip Multiplier: {RearGripMultiplier:F2}"
        );
    }
}