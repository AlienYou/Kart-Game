using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KartPhysics : MonoBehaviour
{
    Rigidbody rb;

    public Vector3 LocalVelocity
    {
        get;
        private set;
    }

    public float ForwardSpeed
    {
        get;
        private set;
    }

    public float LateralSpeed
    {
        get;
        private set;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        CalculateLocalVelocity();
    }

    void CalculateLocalVelocity()
    {
        LocalVelocity = transform.InverseTransformDirection(rb.velocity);
        // z方向 = 车辆前后
        ForwardSpeed = LocalVelocity.z;
        // x方向 = 横向
        LateralSpeed = LocalVelocity.x;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 300, 30), $"ForwardSpeed:{ForwardSpeed:F2}");
        GUI.Label(new Rect(20, 50, 300, 30), $"LateralSpeed:{LateralSpeed:F2}");
    }
}
