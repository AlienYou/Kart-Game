using UnityEngine;

public class KartController : MonoBehaviour
{
    [Header("车辆")]
    public Rigidbody rb;

    [Header("四个轮子")]
    public KartWheel frontLeft;
    public KartWheel frontRight;
    public KartWheel rearLeft;
    public KartWheel rearRight;

    public float acceleration = 5000f;
    public float maxSpeed = 30f;
    public Transform centerOfMass;
    float moveInput;
    float steerInput;
    float forwardSpeed;
    float sideSpeed;

    void Start()
    {
        rb.centerOfMass = centerOfMass.localPosition;
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }



    void FixedUpdate()
    {
        Vector3 force = transform.forward * moveInput * acceleration;
        rb.AddForce(force);
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        forwardSpeed = localVelocity.z;
        sideSpeed = localVelocity.x;
        float steerFactor = 1f - Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);
        float steerPower = Mathf.Lerp(15, 50, steerFactor);
        float turn = steerInput * steerPower * Time.fixedDeltaTime;
        transform.Rotate(0, turn, 0);
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