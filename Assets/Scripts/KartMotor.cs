using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartInput))]
public class KartMotor : MonoBehaviour
{
    Rigidbody rb;
    KartInput input;

    [Header("速度")]
    public float maxForwardSpeed = 25f;
    public float maxReverseSpeed = 8f;

    [Header("加速度")]
    public float acceleration = 35f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<KartInput>();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        float throttle = input.Throttle;

        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

        float currentForwardSpeed = localVelocity.z;

        // 没有油门，并且车辆已接近静止
        if (Mathf.Abs(throttle) < 0.01f && Mathf.Abs(currentForwardSpeed) < 0.1f && Mathf.Abs(localVelocity.x) < 0.05f)
        {
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.velocity, transform.up);
            rb.velocity -= horizontalVelocity;
            return;
        }

        float targetSpeed = 0f;

        if (throttle > 0f)
        {
            targetSpeed = maxForwardSpeed;
        }
        else if (throttle < 0f)
        {
            targetSpeed = -maxReverseSpeed;
        }

        float speedError = targetSpeed - currentForwardSpeed;

        float accel = Mathf.Clamp(speedError, -acceleration, acceleration);

        rb.AddForce(transform.forward * accel * rb.mass, ForceMode.Force);
    }
}
