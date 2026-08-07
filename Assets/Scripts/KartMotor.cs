using UnityEngine;

[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartInput))]
[RequireComponent(typeof(KartPhysics))]
[RequireComponent(typeof(KartController))]
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

    [Range(0f, 1f)]
    [Tooltip("刹车力分配给前轮的比例")]
    public float frontBrakeRatio = 0.6f;

    [Header("轮胎纵向抓地限制")]
    public float driveGripCoefficient = 1.3f;
    public float brakeGripCoefficient = 1.5f;

    [Header("自然阻力")]
    public float rollingResistance = 1.2f;
    public float airResistance = 0.015f;

    [Header("低速停止")]
    public float stopSpeedThreshold = 0.12f;
    public float stopLateralThreshold = 0.08f;

    [Header("调试")]
    public bool showDebugInfo = true;
    public bool drawDebugForces = true;

    public bool IsBraking { get; private set; }
    public bool IsReversing { get; private set; }

    private Rigidbody rb;
    private KartInput input;
    private KartPhysics kartPhysics;
    private KartController controller;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<KartInput>();
        kartPhysics = GetComponent<KartPhysics>();
        controller = GetComponent<KartController>();
    }

    private void FixedUpdate()
    {
        ResetLongitudinalGripData();
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
        float availableAcceleration = forwardAcceleration * (1f - speedRatio) * throttle;
        Vector3 totalForce = transform.forward * availableAcceleration * rb.mass;

        ApplyDriveForceToRearWheels(totalForce);
    }

    private void ApplyReverseDrive(float reverseInput, float forwardSpeed)
    {
        if (forwardSpeed <= -maxReverseSpeed)
        {
            return;
        }

        float reverseSpeed = Mathf.Max(0f, -forwardSpeed);
        float speedRatio = Mathf.Clamp01(reverseSpeed / maxReverseSpeed);
        float availableAcceleration = reverseAcceleration * (1f - speedRatio) * reverseInput;
        Vector3 totalForce = -transform.forward * availableAcceleration * rb.mass;

        ApplyDriveForceToRearWheels(totalForce);
    }

    private void ApplyDriveForceToRearWheels(Vector3 totalForce)
    {
        KartWheel rearLeft = controller.rearLeft;
        KartWheel rearRight = controller.rearRight;

        int groundedWheelCount = 0;

        if (IsWheelGrounded(rearLeft))
        {
            groundedWheelCount++;
        }

        if (IsWheelGrounded(rearRight))
        {
            groundedWheelCount++;
        }

        if (groundedWheelCount == 0)
        {
            return;
        }

        Vector3 forcePerWheel = totalForce / groundedWheelCount;

        ApplyLongitudinalForceToWheel(
            rearLeft,
            forcePerWheel,
            driveGripCoefficient,
            Color.green
        );

        ApplyLongitudinalForceToWheel(
            rearRight,
            forcePerWheel,
            driveGripCoefficient,
            Color.green
        );
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

        Vector3 totalForce = transform.forward * brakeDirection * actualBrakeAcceleration * rb.mass;

        float rearBrakeRatio = 1f - frontBrakeRatio;

        ApplyBrakeToAxle(
            controller.frontLeft,
            controller.frontRight,
            totalForce * frontBrakeRatio
        );

        ApplyBrakeToAxle(
            controller.rearLeft,
            controller.rearRight,
            totalForce * rearBrakeRatio
        );
    }

    private void ApplyBrakeToAxle(KartWheel leftWheel, KartWheel rightWheel, Vector3 axleForce)
    {
        int groundedWheelCount = 0;

        if (IsWheelGrounded(leftWheel))
        {
            groundedWheelCount++;
        }

        if (IsWheelGrounded(rightWheel))
        {
            groundedWheelCount++;
        }

        if (groundedWheelCount == 0)
        {
            return;
        }

        Vector3 forcePerWheel = axleForce / groundedWheelCount;

        ApplyLongitudinalForceToWheel(
            leftWheel,
            forcePerWheel,
            brakeGripCoefficient,
            Color.red
        );

        ApplyLongitudinalForceToWheel(
            rightWheel,
            forcePerWheel,
            brakeGripCoefficient,
            Color.red
        );
    }

    private void ApplyLongitudinalForceToWheel(
    KartWheel wheel,
    Vector3 requestedForce,
    float gripCoefficient,
    Color debugColor)
    {
        if (!IsWheelGrounded(wheel))
        {
            return;
        }

        float maximumTireForce = wheel.suspensionForce * gripCoefficient;

        float lateralUsage = Mathf.Clamp01(wheel.lateralGripUsage);
        float remainingGripRatio = Mathf.Sqrt(
            Mathf.Max(0f, 1f - lateralUsage * lateralUsage)
        );

        float availableLongitudinalForce = maximumTireForce * remainingGripRatio;
        Vector3 actualForce = Vector3.ClampMagnitude(
            requestedForce,
            availableLongitudinalForce
        );

        rb.AddForceAtPosition(actualForce, wheel.hit.point, ForceMode.Force);

        wheel.appliedLongitudinalForce = actualForce;

        if (maximumTireForce > 0.001f)
        {
            wheel.longitudinalGripUsage =
                actualForce.magnitude / maximumTireForce;
        }
        else
        {
            wheel.longitudinalGripUsage = 0f;
        }

        if (drawDebugForces)
        {
            Debug.DrawRay(
                wheel.hit.point,
                actualForce / rb.mass * 0.08f,
                debugColor
            );
        }
    }

    private bool IsWheelGrounded(KartWheel wheel)
    {
        return wheel != null &&
               wheel.wheelPoint != null &&
               wheel.grounded;
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

        Vector3 rollingAcceleration = -movementDirection * rollingResistance;
        rb.AddForce(rollingAcceleration, ForceMode.Acceleration);

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

    private void ResetLongitudinalGripData()
    {
        ResetWheelLongitudinalData(controller.frontLeft);
        ResetWheelLongitudinalData(controller.frontRight);
        ResetWheelLongitudinalData(controller.rearLeft);
        ResetWheelLongitudinalData(controller.rearRight);
    }

    private void ResetWheelLongitudinalData(KartWheel wheel)
    {
        if (wheel == null)
        {
            return;
        }

        wheel.longitudinalGripUsage = 0f;
        wheel.appliedLongitudinalForce = Vector3.zero;
    }

    private void OnGUI()
    {
        if (!showDebugInfo)
        {
            return;
        }

        GUI.Label(
            new Rect(20, 210, 500, 25),
            $"Forward Speed: {kartPhysics.ForwardSpeed:F2} m/s"
        );

        GUI.Label(
            new Rect(20, 225, 500, 25),
            $"Throttle: {input.Throttle:F0}"
        );

        GUI.Label(
            new Rect(20, 250, 500, 25),
            $"Braking: {IsBraking}  Reversing: {IsReversing}"
        );

        GUI.Label(
            new Rect(20, 275, 700, 25),
            $"FL Grip L:{controller.frontLeft.lateralGripUsage:F2} " +
            $"T:{controller.frontLeft.longitudinalGripUsage:F2}"
        );

        GUI.Label(
            new Rect(20, 300, 700, 25),
            $"FR Grip L:{controller.frontRight.lateralGripUsage:F2} " +
            $"T:{controller.frontRight.longitudinalGripUsage:F2}"
        );

        GUI.Label(
            new Rect(20, 325, 700, 25),
            $"RL Grip L:{controller.rearLeft.lateralGripUsage:F2} " +
            $"T:{controller.rearLeft.longitudinalGripUsage:F2}"
        );

        GUI.Label(
            new Rect(20, 350, 700, 25),
            $"RR Grip L:{controller.rearRight.lateralGripUsage:F2} " +
            $"T:{controller.rearRight.longitudinalGripUsage:F2}"
        );
    }
}