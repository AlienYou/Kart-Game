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

        // 当前前向速度
        float currentSpeed = Vector3.Dot(rb.velocity, transform.forward);

        // 根据输入决定目标速度
        float targetSpeed = 0f;

        if (throttle > 0)
            targetSpeed = maxForwardSpeed;

        if (throttle < 0)
            targetSpeed = -maxReverseSpeed;

        // 当前需要补多少速度
        float speedError = targetSpeed - currentSpeed;

        // 计算需要施加的加速度
        float accel = Mathf.Clamp(speedError, -acceleration, acceleration);

        rb.AddForce(transform.forward * accel * rb.mass, ForceMode.Force);
    }
}
