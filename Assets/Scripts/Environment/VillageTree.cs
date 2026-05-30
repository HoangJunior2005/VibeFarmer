using UnityEngine;

/// <summary>
/// Tạo một cây cảnh làng quê Việt Nam procedurally:
/// thân cây (Cylinder) + nhiều lớp tán lá (Sphere).
/// Kéo thả script này lên bất kỳ GameObject nào trong scene.
/// </summary>
[AddComponentMenu("Thôn An Lúa/Environment/Village Tree")]
public class VillageTree : MonoBehaviour
{
    [Header("Thân cây")]
    [Range(0.05f, 0.5f)]  public float trunkRadius   = 0.12f;
    [Range(0.5f,  5f)]    public float trunkHeight    = 2.5f;

    [Header("Tán lá")]
    [Range(0.3f, 3f)]     public float leafRadius     = 1.1f;
    [Range(1,    4)]      public int   leafLayers      = 3;
    [Range(0f,   1f)]     public float leafLayerStep  = 0.35f;   // khoảng cách dọc giữa các lớp
    [Range(0f,   0.4f)]   public float leafLayerShrink = 0.15f;  // thu nhỏ lớp trên

    [Header("Màu sắc")]
    public Color trunkColor = new Color(0.40f, 0.25f, 0.10f);
    public Color leafColor  = new Color(0.18f, 0.52f, 0.18f);

    [Header("Ngẫu nhiên")]
    [Range(0f, 0.3f)] public float positionJitter = 0.05f;
    public int randomSeed = 0;

    // ── Internal ──────────────────────────────────────────────────────────────
    private void Awake() => BuildTree();

    private void BuildTree()
    {
        Random.InitState(randomSeed == 0 ? GetInstanceID() : randomSeed);

        // ── Thân cây ──
        GameObject trunk = new GameObject("Trunk");
        trunk.transform.SetParent(transform, false);
        var mf  = trunk.AddComponent<MeshFilter>();
        var mr  = trunk.AddComponent<MeshRenderer>();
        mf.sharedMesh        = ProceduralMeshHelper.CreateCylinder(trunkRadius, trunkHeight, 10);
        mr.sharedMaterial    = ProceduralMeshHelper.CreateMaterial(trunkColor);

        // Collider cho thân
        var col = trunk.AddComponent<CapsuleCollider>();
        col.height = trunkHeight;
        col.radius = trunkRadius;
        col.center = new Vector3(0, trunkHeight * 0.5f, 0);

        // ── Tán lá nhiều lớp ──
        float baseY = trunkHeight * 0.75f;
        for (int layer = 0; layer < leafLayers; layer++)
        {
            float r = leafRadius - layer * leafLayerShrink;
            float y = baseY + layer * leafLayerStep * leafRadius;
            // offset nhỏ cho tự nhiên
            Vector3 offset = new Vector3(
                Random.Range(-positionJitter, positionJitter),
                0,
                Random.Range(-positionJitter, positionJitter));

            GameObject leafBall = new GameObject($"Leaf_L{layer}");
            leafBall.transform.SetParent(transform, false);
            leafBall.transform.localPosition = new Vector3(0, y, 0) + offset;

            var lmf = leafBall.AddComponent<MeshFilter>();
            var lmr = leafBall.AddComponent<MeshRenderer>();
            lmf.sharedMesh = ProceduralMeshHelper.CreateSphere(r, 8, 12);
            // màu xanh biến thể nhẹ theo từng lớp
            Color c = leafColor + new Color(
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f));
            lmr.sharedMaterial = ProceduralMeshHelper.CreateMaterial(c);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Xoá rồi dựng lại khi thay đổi trong Inspector
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);
        BuildTree();
    }
#endif
}
