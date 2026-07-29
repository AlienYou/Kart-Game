using UnityEngine;

public class KartController : MonoBehaviour
{
    public Rigidbody rb;
    public float acceleration = 5000f;
    public float maxSpeed = 30f;
    public float steering = 50f;
    float moveInput;
    float steerInput;


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
        float turn = steerInput * steering * Time.fixedDeltaTime;
        transform.Rotate(0, turn, 0);
    }
}