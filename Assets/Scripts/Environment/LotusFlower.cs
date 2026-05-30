using UnityEngine;

/// <summary>
/// Tạo một bông sen hoàn chỉnh (lá + hoa) procedurally.
/// Búp sen nở ban ngày (6h-18h), khép ban đêm — dùng TimeManager.Hour.
/// </summary>
[AddComponentMenu("Thôn An Lúa/Environment/Lotus Flower")]
public class LotusFlower : MonoBehaviour
{
    [Header("Lá sen")]
    [Range(0.3f, 1.2f)] public float leafRadius   = 0.55f;
    [Range(1, 5)]        public int   leafCount    = 4;
    public Color leafColor = new Color(0.15f, 0.55f, 0.20f);

    [Header("Hoa")]
    [Range(0.05f, 0.4f)] public float petalRadius = 0.18f;
    [Range(0.1f, 0.6f)]  public float petalHeight = 0.38f;
    [Range(3, 8)]         public int   petalCount = 6;
    public Color petalColor = new Color(0.98f, 0.55f, 0.72f);  // Hồng
    public Color stamenColor = new Color(1f, 0.93f, 0.2f);     // Nhụy vàng

    [Header("Cuống")]
    [Range(0.01f, 0.06f)] public float stemRadius = 0.025f;
    [Range(0.1f, 0.6f)]   public float stemHeight = 0.35f;
    public Color stemColor = new Color(0.20f, 0.48f, 0.18f);

    [Header("Animation nở")]
    [Tooltip("Tốc độ chuyển đổi nở/khép (0 = tức thì)")]
    [Range(0f, 2f)] public float bloomSpeed = 0.8f;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private Transform _petalRoot;
    private float     _targetBloom = 1f;   // 0 = khép, 1 = nở hoàn toàn
    private float     _currentBloom = 1f;

    private void Start()
    {
        BuildLotus();
        // Đăng ký sự kiện giờ thay đổi từ TimeManager
        TimeManager.OnHourChanged += OnHourChanged;
        // Áp dụng trạng thái ngay
        int hour = TimeManager.Hour;
        _currentBloom = _targetBloom = (hour >= 6 && hour < 19) ? 1f : 0f;
        ApplyBloom(_currentBloom);
    }

    private void OnDestroy()
    {
        TimeManager.OnHourChanged -= OnHourChanged;
    }

    private void OnHourChanged()
    {
        int hour = TimeManager.Hour;
        _targetBloom = (hour >= 6 && hour < 19) ? 1f : 0f;
    }

    private void Update()
    {
        if (Mathf.Approximately(_currentBloom, _targetBloom)) return;
        _currentBloom = Mathf.MoveTowards(_currentBloom, _targetBloom, bloomSpeed * Time.deltaTime);
        ApplyBloom(_currentBloom);
    }

    // Cánh hoa xoay ra/vào theo trạng thái nở
    private void ApplyBloom(float t)
    {
        if (_petalRoot == null) return;
        // Cánh đứng = khép (90°), cánh ngang = nở (30°)
        float angle = Mathf.Lerp(85f, 28f, t);
        foreach (Transform petal in _petalRoot)
            petal.localRotation = Quaternion.Euler(angle, petal.localRotation.eulerAngles.y, 0);
    }

    // ── Build ─────────────────────────────────────────────────────────────────
    private void BuildLotus()
    {
        Random.InitState(GetInstanceID());

        // -- Lá sen (nổi trên mặt nước) --
        for (int i = 0; i < leafCount; i++)
        {
            float angle  = 360f * i / leafCount + Random.Range(-15f, 15f);
            float dist   = leafRadius * Random.Range(0.5f, 0.95f);
            Vector3 pos  = Quaternion.Euler(0, angle, 0) * Vector3.forward * dist;
            pos.y        = 0.02f;

            GameObject leaf = new GameObject($"Leaf_{i}");
            leaf.transform.SetParent(transform, false);
            leaf.transform.localPosition = pos;
            leaf.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            var lmf = leaf.AddComponent<MeshFilter>();
            var lmr = leaf.AddComponent<MeshRenderer>();
            float r  = leafRadius * Random.Range(0.6f, 1f);
            lmf.sharedMesh = ProceduralMeshHelper.CreateDisk(r, 16);
            lmr.sharedMaterial = ProceduralMeshHelper.CreateMaterial(
                leafColor + new Color(Random.Range(-0.04f,0.04f), Random.Range(-0.04f,0.04f), 0));
        }

        // -- Cuống hoa --
        GameObject stem = new GameObject("Stem");
        stem.transform.SetParent(transform, false);
        stem.transform.localPosition = Vector3.zero;
        var smf = stem.AddComponent<MeshFilter>();
        var smr = stem.AddComponent<MeshRenderer>();
        smf.sharedMesh = ProceduralMeshHelper.CreateCylinder(stemRadius, stemHeight, 8);
        smr.sharedMaterial = ProceduralMeshHelper.CreateMaterial(stemColor);

        // -- Cánh hoa (pivot ở đáy, xoay quanh tâm) --
        _petalRoot = new GameObject("PetalRoot").transform;
        _petalRoot.SetParent(transform, false);
        _petalRoot.localPosition = new Vector3(0, stemHeight, 0);

        for (int i = 0; i < petalCount; i++)
        {
            float yAngle = 360f * i / petalCount;

            GameObject petal = new GameObject($"Petal_{i}");
            petal.transform.SetParent(_petalRoot, false);
            petal.transform.localRotation = Quaternion.Euler(28f, yAngle, 0);

            var pmf = petal.AddComponent<MeshFilter>();
            var pmr = petal.AddComponent<MeshRenderer>();
            // Cánh = cone hướng lên, pivot ở đáy
            pmf.sharedMesh = ProceduralMeshHelper.CreateCone(petalRadius, petalHeight, 8);
            pmr.sharedMaterial = ProceduralMeshHelper.CreateMaterial(petalColor);
        }

        // -- Nhụy (stamen) ở giữa --
        GameObject stamen = new GameObject("Stamen");
        stamen.transform.SetParent(_petalRoot, false);
        stamen.transform.localPosition = Vector3.zero;
        var stamf = stamen.AddComponent<MeshFilter>();
        var stamr = stamen.AddComponent<MeshRenderer>();
        stamf.sharedMesh = ProceduralMeshHelper.CreateSphere(petalRadius * 0.35f, 6, 8);
        stamr.sharedMaterial = ProceduralMeshHelper.CreateMaterial(stamenColor);
    }
}
