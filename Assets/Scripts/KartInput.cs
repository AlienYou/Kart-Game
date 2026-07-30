using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartInput : MonoBehaviour
{
    public float Throttle { get; private set; }
    public float Steering { get; private set; }

    public bool Drift { get; private set; }
    public bool Nitro { get; private set; }
    public bool Brake { get; private set; }

    void Update()
    {
        // W/S
        Throttle = Input.GetAxisRaw("Vertical");
        // A/D
        Steering = Input.GetAxisRaw("Horizontal");
        Brake = Input.GetKey(KeyCode.Space);
        Drift = Input.GetKey(KeyCode.LeftShift);
        Nitro = Input.GetKey(KeyCode.LeftControl);
    }
}
