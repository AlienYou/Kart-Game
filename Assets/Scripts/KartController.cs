using UnityEngine;

public class KartController : MonoBehaviour
{
    public Rigidbody rb;
    public float acceleration = 5000f;
    public float maxSpeed = 30f;
    public float steering = 50f;
    float moveInput;
    float steerInput;
    float forwardSpeed;
    float sideSpeed;


    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }



    void FixedUpdate()
    {
        Vector3 force = transform.forward * moveInput * acceleration;
        rb.AddForce(force);
        if(rb.velocity.magnitude > maxSpeed)
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
}