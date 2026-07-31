using UnityEngine;

[System.Serializable]
public class KartWheel
{
    [Header("节点")]
    public Transform wheelPoint;

    [Tooltip("负责悬挂位置和前轮转向的节点")]
    public Transform wheelVisualRoot;

    [Tooltip("真正滚动的轮胎模型")]
    public Transform wheelMesh;

    [Header("轮胎参数")]
    [Min(0.01f)]
    public float radius = 0.25f;

    [Min(0.01f)]
    public float suspensionLength = 0.30f;

    [Tooltip("轮胎模型绕哪个局部轴滚动。通常为 X 轴")]
    public Vector3 rotationAxis = Vector3.right;

    [Header("轮胎类型")]
    public bool isFrontWheel;
    public bool isLeftWheel;

    [Header("运行时数据")]
    [HideInInspector] public bool grounded;
    [HideInInspector] public RaycastHit hit;
    [HideInInspector] public float compression;
    [HideInInspector] public float currentSuspensionLength;
    [HideInInspector] public float previousSuspensionLength;
    [HideInInspector] public float suspensionVelocity;
    [HideInInspector] public float suspensionForce;
    [HideInInspector] public float visualRotationAngle;
    [HideInInspector] public Quaternion wheelMeshBaseRotation;
}