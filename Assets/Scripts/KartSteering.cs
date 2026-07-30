using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(KartInput))]
public class KartSteering : MonoBehaviour
{
    Rigidbody rb;
    KartInput input;

    [Header("转向")]
    public float maxSteerAngle = 30f;
    public float steerSpeed = 8f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<KartInput>();
    }

    void FixedUpdate()
    {
        Steering();
    }
    
    void Steering()
    {
        if (rb.velocity.sqrMagnitude < 0.1f)
        {
            return;
        }
        //当前速度
        float speed = rb.velocity.magnitude;
        //高速降低转向角
        float speedRatio = Mathf.Clamp01(speed / 25f);
        float steerAngle = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.35f, speedRatio);

        //根据输入得到本帧旋转角度
        float yaw = input.Steering * steerAngle * steerSpeed * Time.fixedDeltaTime;

        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        rb.MoveRotation(rb.rotation * rotation);
    }
}
