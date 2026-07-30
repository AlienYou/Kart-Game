using UnityEngine;

[System.Serializable]
public class KartWheel
{
    [Header("节点")]
    public Transform wheelPoint;
    public Transform wheelMesh;

    [Header("轮胎参数")]
    [Min(0.01f)]
    public float radius = 0.25f;

    [Min(0f)]
    public float suspensionLength = 0.30f;

    [Header("轮胎类型")]
    public bool isFrontWheel;
    public bool isLeftWheel;

    [HideInInspector]
    public bool grounded;

    [HideInInspector]
    public RaycastHit hit;

    [HideInInspector]
    public float compression;
}