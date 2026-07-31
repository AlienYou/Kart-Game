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
    public Transform centerOfMass;

    [Header("四个轮子")]
    public KartWheel frontLeft;
    public KartWheel frontRight;
    public KartWheel rearLeft;
    public KartWheel rearRight;

    [Header("车辆基础参数")]
    public float maxSpeed = 30f;
    public float acceleration = 40f;
    public float brakeForce = 50f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<KartInput>();
        kartPhysics = GetComponent<KartPhysics>();

        ConfigureWheels();
        ConfigureCenterOfMass();
    }

    private void ConfigureWheels()
    {
        if (frontLeft != null)
        {
            frontLeft.isFrontWheel = true;
            frontLeft.isLeftWheel = true;
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

    private void ConfigureCenterOfMass()
    {
        if (centerOfMass != null)
        {
            rb.centerOfMass = centerOfMass.localPosition;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (centerOfMass == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(centerOfMass.position, 0.06f);
    }
}