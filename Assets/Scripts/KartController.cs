using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartInput))]
[RequireComponent(typeof(KartPhysics))]
public class KartController : MonoBehaviour
{
    [Header("车辆组件")]
    public Rigidbody rb;
    public KartInput input;
    public KartPhysics kartPhysics;

    [Header("四个轮子")]
    public KartWheel frontLeft;
    public KartWheel frontRight;
    public KartWheel rearLeft;
    public KartWheel rearRight;

    [Header("车辆参数")]
    public float acceleration = 5000f;
    public float maxSpeed = 30f;
    public float brakeForce = 50f;
    public Transform centerOfMass;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<KartInput>();
        kartPhysics = GetComponent<KartPhysics>();
    }

    void ConfigureWheels()
    {
        if (frontLeft != null)
        {
            frontLeft.isFrontWheel = true;
            frontRight.isLeftWheel = true;
        }
        if (frontRight != null)
        {
            frontRight.isFrontWheel = true;
            frontRight.isLeftWheel = false;
        }
        if (rearLeft != null)
        {
            rearLeft.isFrontWheel = false;
            rearLeft.isLeftWheel = true;    
        }
        if (rearRight != null)
        {
            rearRight.isFrontWheel = false;
            rearRight.isLeftWheel = false;
        }
    }

    void Start()
    {
        rb.centerOfMass = centerOfMass.localPosition;
    }

    void FixedUpdate()
    {
        // Vector3 force = transform.forward * moveInput * acceleration;
        // rb.AddForce(force);
        // if (rb.velocity.magnitude > maxSpeed)
        // {
        //     rb.velocity = rb.velocity.normalized * maxSpeed;
        // }
        // Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        // forwardSpeed = localVelocity.z;
        // sideSpeed = localVelocity.x;
        // float steerFactor = 1f - Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);
        // float steerPower = Mathf.Lerp(15, 50, steerFactor);
        // float turn = steerInput * steerPower * Time.fixedDeltaTime;
        // transform.Rotate(0, turn, 0);
    }

    void OnDrawGizmos()
    {
        if (centerOfMass == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(centerOfMass.position, 0.08f);
    }
}