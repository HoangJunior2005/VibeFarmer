using UnityEngine;

/// <summary>
/// Đặt nhiều cây làng quê tự động vào scene theo danh sách vị trí hoặc
/// phân bổ ngẫu nhiên trong một vùng hình chữ nhật.
/// </summary>
[AddComponentMenu("Thôn An Lúa/Environment/Village Tree Spawner")]
public class VillageTreeSpawner : MonoBehaviour
{
    public enum SpawnMode { FixedPositions, RandomArea }

    [Header("Chế độ đặt cây")]
    public SpawnMode spawnMode = SpawnMode.RandomArea;

    [Header("Vị trí cố định (FixedPositions)")]
    [Tooltip("Danh sách vị trí thế giới để đặt cây")]
    public Vector3[] fixedPositions = new Vector3[]
    {
        new Vector3(-8f, 0f, 5f),
        new Vector3(-5f, 0f, 8f),
        new Vector3(10f, 0f, -3f),
        new Vector3(12f, 0f,  2f),
        new Vector3(-10f, 0f, -6f),
    };

    [Header("Vùng ngẫu nhiên (RandomArea)")]
    [Tooltip("Trung tâm vùng đặt cây")]
    public Vector3 areaCenter = Vector3.zero;
    [Tooltip("Kích thước vùng (X x Z)")]
    public Vector2 areaSize   = new Vector2(30f, 30f);
    [Range(3, 40)]
    public int treeCount       = 12;

    [Header("Cây")]
    [Tooltip("Tuỳ chỉnh cây mẫu — để trống để dùng mặc định")]
    public VillageTree treePrefab;

    [Range(0.6f, 2f)]   public float minScale = 0.8f;
    [Range(0.6f, 3f)]   public float maxScale = 1.4f;

    [Header("Bù chiều cao (Raycast)")]
    [Tooltip("Bật để cây tự đứng đúng độ cao mặt đất")]
    public bool snapToGround    = true;
    public LayerMask groundMask = ~0;

    [Header("Seed")]
    public int seed = 42;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start() => Spawn();

    private void Spawn()
    {
        Random.InitState(seed);

        if (spawnMode == SpawnMode.FixedPositions)
        {
            foreach (var pos in fixedPositions)
                SpawnOneTree(pos);
        }
        else
        {
            for (int i = 0; i < treeCount; i++)
            {
                Vector3 pos = areaCenter + new Vector3(
                    Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                    0,
                    Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f));
                SpawnOneTree(pos);
            }
        }
    }

    private void SpawnOneTree(Vector3 worldPos)
    {
        // Snap xuống mặt đất
        if (snapToGround)
        {
            Ray ray = new Ray(worldPos + Vector3.up * 20f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 40f, groundMask))
                worldPos.y = hit.point.y;
        }

        GameObject treeGO;
        if (treePrefab != null)
        {
            treeGO = Instantiate(treePrefab.gameObject, worldPos, Quaternion.identity, transform);
        }
        else
        {
            treeGO = new GameObject("VillageTree");
            treeGO.transform.SetParent(transform);
            treeGO.transform.position = worldPos;
            var tree = treeGO.AddComponent<VillageTree>();
            // tuỳ biến nhẹ mỗi cây
            tree.trunkHeight   = Random.Range(1.8f, 3.2f);
            tree.trunkRadius   = Random.Range(0.08f, 0.16f);
            tree.leafRadius    = Random.Range(0.8f, 1.4f);
            tree.leafLayers    = Random.Range(2, 4);
            tree.randomSeed    = Random.Range(1, 9999);
        }

        // Scale ngẫu nhiên
        float s = Random.Range(minScale, maxScale);
        treeGO.transform.localScale = Vector3.one * s;
        // Xoay ngẫu nhiên quanh Y
        treeGO.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        if (spawnMode == SpawnMode.RandomArea)
            Gizmos.DrawCube(areaCenter, new Vector3(areaSize.x, 0.1f, areaSize.y));
        else
            foreach (var p in fixedPositions)
                Gizmos.DrawWireSphere(p, 0.4f);
    }
#endif
}
