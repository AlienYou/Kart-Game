using UnityEngine;
using UnityEngine.Timeline;

[System.Serializable]
public class KartWheel
{
    [Header("节点")]
    public Transform wheelPoint;
    public Transform wheelMesh;

    [Header("轮胎参数")]
    [Min(0.01f)]
    public float radius = 0.25f;

    [Header("悬挂从完全收缩到完全伸展的可移动距离")]
    [Min(0.01f)]
    public float suspensionLength = 0.30f;

    [Header("轮胎类型")]
    public bool isFrontWheel;
    public bool isLeftWheel;

    [Header("运行时数据")]
    [HideInInspector]
    public bool grounded;
    [HideInInspector]
    public RaycastHit hit;
    [HideInInspector]
    public float compression;
    [HideInInspector]
    public float currentSuspensionLength;

    [HideInInspector]
    public float previousSuspensionLength;

    [HideInInspector]
    public float suspensionVelocity;

    [HideInInspector]
    public float suspensionForce;
}