using UnityEngine;

/// <summary>
/// Tạo nhiều bông sen phân bổ trên mặt hồ / ao trong một vùng hình tròn.
/// </summary>
[AddComponentMenu("Thôn An Lúa/Environment/Lotus Spawner")]
public class LotusSpawner : MonoBehaviour
{
    [Header("Vùng đặt sen")]
    [Tooltip("Bán kính vùng sen (thường bằng bán kính mặt hồ)")]
    [Range(0.5f, 20f)] public float pondRadius = 4f;

    [Header("Số lượng")]
    [Range(1, 30)] public int lotusCount = 8;

    [Header("Kích thước")]
    [Range(0.5f, 2f)] public float minScale = 0.7f;
    [Range(0.5f, 2f)] public float maxScale = 1.3f;

    [Header("Màu hoa (override ngẫu nhiên)")]
    [Tooltip("Thêm màu hoa hồng/trắng xen lẫn")]
    public bool randomPetalColor = true;
    public Color petalColorA = new Color(0.98f, 0.55f, 0.72f); // hồng
    public Color petalColorB = new Color(0.98f, 0.95f, 0.95f); // trắng ngà

    [Header("Seed")]
    public int seed = 7;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start() => SpawnLotus();

    private void SpawnLotus()
    {
        Random.InitState(seed);

        for (int i = 0; i < lotusCount; i++)
        {
            // Phân bổ đều trong vòng tròn (Poisson-lite)
            float angle = Random.Range(0f, 360f);
            float dist  = Mathf.Sqrt(Random.Range(0f, 1f)) * pondRadius * 0.85f;
            Vector3 pos = transform.position + Quaternion.Euler(0, angle, 0) * Vector3.forward * dist;
            pos.y = transform.position.y;  // nằm trên mặt nước

            GameObject lotusGO = new GameObject($"Lotus_{i}");
            lotusGO.transform.SetParent(transform, false);
            lotusGO.transform.position = pos;
            lotusGO.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            lotusGO.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);

            var lotus = lotusGO.AddComponent<LotusFlower>();

            if (randomPetalColor)
                lotus.petalColor = Random.value > 0.4f ? petalColorA : petalColorB;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.5f, 0.7f, 0.3f);
        DrawCircle(transform.position, pondRadius, 32);
    }

    private void DrawCircle(Vector3 center, float r, int segs)
    {
        Vector3 prev = center + new Vector3(r, 0, 0);
        for (int i = 1; i <= segs; i++)
        {
            float a  = Mathf.PI * 2f * i / segs;
            Vector3 next = center + new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}
