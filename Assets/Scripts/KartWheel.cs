using UnityEngine;

[System.Serializable]
public class KartWheel
{
    [Header("节点")]
    public Transform wheelPoint;
    public Transform wheelMesh;

    [Header("轮胎参数")]
    public float radius = 0.25f;
    public float suspensionLength = 0.30f;

    [HideInInspector]
    public bool grounded;

    [HideInInspector]
    public RaycastHit hit;

    [HideInInspector]
    public float compression;
}