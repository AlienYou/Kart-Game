using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartInput))]
[RequireComponent(typeof(KartPhysics))]
public class KartMotor : MonoBehaviour
{
    [Header("前进动力")]
    public float maxForwardSpeed = 25f;
    public float forwardAcceleration = 8f;

    [Header("倒车动力")]
    public float maxReverseSpeed = 8f;
    public float reverseAcceleration = 5f;

    [Header("刹车")]
    public float brakeAcceleration = 18f;
    public float reverseEnterSpeed = 0.5f;

    [Header("自然阻力")]
    public float rollingResistance = 1.2f;
    public float airResistance = 0.015f;

    [Header("低速停止")]
    public float stopSpeedThreshold = 0.12f;
    public float stopLateralThreshold = 0.08f;

    [Header("调试")]
    public bool showDebugInfo = true;

    public bool IsBraking { get; private set; }
    public bool IsReversing { get; private set; }

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
        UpdateDrivingState();
        ApplyDriveForce();
        ApplyNaturalResistance();
        ApplyLowSpeedStop();
    }

    private void UpdateDrivingState()
    {
        float throttle = input.Throttle;
        float forwardSpeed = kartPhysics.ForwardSpeed;

        IsBraking = false;
        IsReversing = false;

        if (throttle < -0.01f)
        {
            if (forwardSpeed > reverseEnterSpeed)
            {
                IsBraking = true;
            }
            else
            {
                IsReversing = true;
            }

            return;
        }

        if (throttle > 0.01f && forwardSpeed < -reverseEnterSpeed)
        {
            IsBraking = true;
        }
    }

    private void ApplyDriveForce()
    {
        float throttle = input.Throttle;
    float forwardSpeed = kartPhysics.ForwardSpeed;

    if (IsBraking)
    {
        ApplyBrakeAgainstCurrentDirection(forwardSpeed);
        return;
    }

    if (throttle > 0.01f)
    {
        ApplyForwardDrive(throttle, forwardSpeed);
        return;
    }

    if (IsReversing)
    {
        ApplyReverseDrive(-throttle, forwardSpeed);
    }
    }

    private void ApplyForwardDrive(float throttle, float forwardSpeed)
    {
        if (forwardSpeed >= maxForwardSpeed)
        {
            return;
        }

        float speedRatio = Mathf.Clamp01(Mathf.Max(0f, forwardSpeed) / maxForwardSpeed);
        float availableAcceleration = forwardAcceleration * (1f - speedRatio);

        Vector3 force = transform.forward * availableAcceleration * throttle;
        rb.AddForce(force, ForceMode.Acceleration);
    }

    private void ApplyReverseDrive(float reverseInput, float forwardSpeed)
    {
        if (forwardSpeed <= -maxReverseSpeed)
        {
            return;
        }

        float reverseSpeed = Mathf.Max(0f, -forwardSpeed);
        float speedRatio = Mathf.Clamp01(reverseSpeed / maxReverseSpeed);
        float availableAcceleration = reverseAcceleration * (1f - speedRatio);

        Vector3 force = -transform.forward * availableAcceleration * reverseInput;
        rb.AddForce(force, ForceMode.Acceleration);
    }

    private void ApplyBrakeAgainstCurrentDirection(float forwardSpeed)
    {
        if (Mathf.Abs(forwardSpeed) < 0.001f)
        {
            return;
        }

        float requiredAcceleration = Mathf.Abs(forwardSpeed) / Time.fixedDeltaTime;
        float actualBrakeAcceleration = Mathf.Min(brakeAcceleration, requiredAcceleration);

        float brakeDirection = -Mathf.Sign(forwardSpeed);
        rb.AddForce(transform.forward * brakeDirection * actualBrakeAcceleration, ForceMode.Acceleration);
    }

    private void ApplyNaturalResistance()
    {
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.velocity, transform.up);
        float horizontalSpeed = horizontalVelocity.magnitude;

        if (horizontalSpeed < 0.001f)
        {
            return;
        }

        Vector3 movementDirection = horizontalVelocity.normalized;

        Vector3 rollingForce = -movementDirection * rollingResistance;
        rb.AddForce(rollingForce, ForceMode.Acceleration);

        float aerodynamicAcceleration = horizontalSpeed * horizontalSpeed * airResistance;
        Vector3 aerodynamicForce = -movementDirection * aerodynamicAcceleration;
        rb.AddForce(aerodynamicForce, ForceMode.Acceleration);
    }

    private void ApplyLowSpeedStop()
    {
        if (Mathf.Abs(input.Throttle) > 0.01f)
        {
            return;
        }

        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

        bool forwardAlmostStopped = Mathf.Abs(localVelocity.z) < stopSpeedThreshold;
        bool lateralAlmostStopped = Mathf.Abs(localVelocity.x) < stopLateralThreshold;

        if (!forwardAlmostStopped || !lateralAlmostStopped)
        {
            return;
        }

        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.velocity, transform.up);
        rb.velocity -= horizontalVelocity;
    }

    private void OnGUI()
    {
        if (!showDebugInfo)
        {
            return;
        }

        GUI.Label(new Rect(20, 210, 400, 25),
            $"Forward Speed: {kartPhysics.ForwardSpeed:F2} m/s");

        GUI.Label(new Rect(20, 225, 400, 25),
            $"Throttle: {input.Throttle:F0}");

        GUI.Label(new Rect(20, 250, 400, 25),
            $"Braking: {IsBraking}  Reversing: {IsReversing}");
    }
}