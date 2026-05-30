using UnityEngine;

/// <summary>
/// EnvironmentManager — Script trung tâm.
/// Kéo thả vào một GameObject trống trong TownScene.
/// Tự động tạo:
///   • Hồ cá với sen (FishPond)
///   • Cụm sen độc lập (LotusSpawner)
///
/// Có thể bật/tắt từng phần qua Inspector.
/// </summary>
[AddComponentMenu("Thôn An Lúa/Environment/Environment Manager")]
public class EnvironmentManager : MonoBehaviour
{
    // ══ HỒ CÁ ════════════════════════════════════════════════════════════════
    [Header("🐟 Hồ Cá")]
    public bool enableFishPond = true;
    [Tooltip("Vị trí trung tâm hồ — chỉnh trong Inspector cho vừa với scene")]
    public Vector3 pondPosition  = new Vector3(10f, 0f, 8f);
    [Range(2f, 12f)] public float pondRadius = 3.5f;
    [Range(0, 12)] public int pondFishCount  = 8;
    [Tooltip("Số lượng sen tập trung tại hồ cá")]
    [Range(0, 30)] public int pondLotusCount = 12;
    public int pondSeed = 99;

    // ══ HỒ PHỤ (thêm hồ thứ hai nhỏ hơn nếu muốn) ════════════════════════
    [Header("🐟 Hồ Phụ (tuỳ chọn)")]
    public bool enableSecondPond = false;
    public Vector3 secondPondPosition = new Vector3(-10f, 0f, -5f);
    [Range(1f, 8f)] public float secondPondRadius = 2.5f;

    // ══ SEN ĐỘC LẬP ══════════════════════════════════════════════════════════
    [Header("🌸 Cụm Sen Độc Lập")]
    [Tooltip("Để sen tập trung hết vào hồ cá tránh lộn xộn, đã tắt hoàn toàn cụm sen trên cạn")]
    public bool enableLotusClusters = false; // Tắt mặc định để hoa sen tập trung hết về hồ cá theo yêu cầu
    public LotusClusterConfig[] lotusClusters = new LotusClusterConfig[]
    {
        new LotusClusterConfig { position = new Vector3( 12f, 0f, -5f), radius = 2.0f, count = 5, seed = 11 },
        new LotusClusterConfig { position = new Vector3(-6f,  0f, 14f), radius = 1.5f, count = 4, seed = 55 },
    };

    // ══ TIỆN ÍCH DỄ TÌM KIẾM ════════════════════════════════════════════════
    [Header("🔧 Vị Trí & Tiện Ích")]
    [Tooltip("Tự động di chuyển Hồ Cá đến ngay cạnh Người Chơi khi bắt đầu game để nhìn thấy ngay lập tức")]
    public bool spawnNearPlayer = true;
    [Tooltip("Khoảng cách lệch từ Người Chơi đến Hồ Cá")]
    public Vector3 playerOffset = new Vector3(8f, 0f, 8f);

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        // Chuyển toàn bộ khởi tạo từ Awake sang Start để bảo đảm:
        // 1. Hệ thống Physics của Unity đã nạp xong hoàn toàn -> Raycast dò mặt đất hoạt động chuẩn 100%.
        // 2. Nhân vật Player đã được đặt cố định trên mặt đất -> Đo vị trí chuẩn.
        // 3. Tránh hoàn toàn lỗi "SendMessage cannot be called during Awake".

        if (spawnNearPlayer)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                Vector3 pPos = player.transform.position;
                // Lấy độ cao Y của Player làm mốc ban đầu để đảm bảo hồ luôn nằm đúng mặt đất
                pondPosition = new Vector3(pPos.x + playerOffset.x, pPos.y, pPos.z + playerOffset.z);
                Debug.Log($"[EnvironmentManager] Tự động chọn vị trí hồ cá theo Player: {pondPosition}");
            }
            else
            {
                Debug.LogWarning("[EnvironmentManager] Không tìm thấy PlayerController trong scene, sử dụng vị trí mặc định.");
            }
        }

        if (enableFishPond)       BuildFishPond(pondPosition, pondRadius, pondFishCount, pondLotusCount, pondSeed);
        if (enableSecondPond)     BuildFishPond(secondPondPosition, secondPondRadius, 4, 3, pondSeed + 1);
        
        // Đã xoá bỏ hoàn toàn ruộng bậc thang theo yêu cầu của bạn!
    }



    // ── Hồ cá ─────────────────────────────────────────────────────────────────
    private void BuildFishPond(Vector3 worldPos, float radius, int fish, int lotus, int s)
    {
        GameObject root = new GameObject("[FishPond]");
        
        // Auto-snap Y coordinate to ground/terrain height
        Ray ray = new Ray(new Vector3(worldPos.x, 100f, worldPos.z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            worldPos.y = hit.point.y;
            Debug.Log($"[EnvironmentManager] Raycast hồ cá thành công! Đất tại Y = {worldPos.y}");
        }
        else
        {
            // Fallback: Nếu raycast không trúng gì, giữ nguyên độ cao Y của Player/Mặc định
            Debug.LogWarning($"[EnvironmentManager] Raycast hồ cá không trúng mặt đất, giữ độ cao Y mặc định = {worldPos.y}");
        }

        root.transform.position = worldPos;
        // Sử dụng SetParent(..., true) để giữ nguyên vị trí thế giới tuyệt đối vừa snap, 
        // không bị ảnh hưởng bởi scale/rotation của GameObject cha Environment.
        root.transform.SetParent(transform, true);

        var pond = root.AddComponent<FishPond>();
        pond.pondRadius   = radius;
        pond.fishCount    = fish;
        pond.lotusCount   = lotus;
        pond.spawnLotus   = lotus > 0;
        pond.enableRipples= true;
        pond.seed         = s;

        Debug.Log($"[EnvironmentManager] Đã dựng Hồ Cá tại Vị trí thế giới: {root.transform.position}");
    }
}

// ── Data classes ──────────────────────────────────────────────────────────────
[System.Serializable]
public class LotusClusterConfig
{
    public Vector3 position;
    [Range(0.5f, 10f)] public float radius = 2f;
    [Range(1, 20)]     public int   count  = 5;
    public int seed = 1;
}
