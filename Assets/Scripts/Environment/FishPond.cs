using UnityEngine;

/// <summary>
/// Tạo hồ cá làng quê Việt Nam hoàn chỉnh dưới dạng hồ sen nổi (Raised Pond):
/// - Nền hồ nổi cao hẳn lên khỏi mặt đất để tránh bị địa hình che lấp hoàn toàn.
/// - Bờ đất thoải (Raised Bank) dốc từ đỉnh hồ xuống tiếp giáp mặt đất.
/// - Đáy hồ và lòng hồ được nâng lên trên mặt đất, mặt trong thành hồ hướng vào trong (Inward Cylinder).
/// - Mặt nước xanh trong nằm sát trên miệng hồ, đàn cá chép bơi lội và hoa sen nổi tuyệt đẹp.
/// </summary>
[AddComponentMenu("Thôn An Lúa/Environment/Fish Pond")]
public class FishPond : MonoBehaviour
{
    [Header("Kích thước hồ")]
    [Range(2f, 20f)] public float pondRadius = 4f;
    [Tooltip("Chiều cao bờ hồ nhô lên trên mặt đất")]
    [Range(0.3f, 2f)] public float pondDepth = 0.5f;

    [Header("Màu sắc")]
    public Color waterColor  = new Color(0.12f, 0.52f, 0.70f, 0.42f); // xanh ngọc lam bán trong suốt (alpha = 0.42f)
    public Color waterEdge   = new Color(0.08f, 0.38f, 0.55f, 0.42f); 
    public Color bottomColor = new Color(0.18f, 0.28f, 0.12f, 1f); // bùn/đáy rêu xanh thẫm
    public Color bankColor   = new Color(0.45f, 0.30f, 0.15f, 1f); // đất nâu làng quê Việt Nam

    [Header("Cá Chép")]
    [Range(0, 20)] public int fishCount  = 8;
    public Color fishColorA = new Color(0.95f, 0.35f, 0.05f); // cá chép đỏ/cam hồng
    public Color fishColorB = new Color(0.90f, 0.75f, 0.15f); // cá chép vàng kim
    [Range(0.1f, 0.5f)] public float fishSize = 0.22f;

    [Header("Gợn nước")]
    public bool enableRipples = true;

    [Header("Sen quanh hồ")]
    public bool spawnLotus  = true;
    [Range(0, 30)] public int lotusCount = 12;

    [Header("Seed")]
    public int seed = 99;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        Random.InitState(seed);

        // 1. Tự động xoá cỏ trên địa hình (Terrain Grass Details) - Tăng bán kính xoá rộng hơn rìa ao bờ thoải
        ClearTerrainDetails(transform.position, pondRadius + 1.2f);

        // 2. Tự động tìm và huỷ bỏ cỏ dạng GameObject/Prefab cắm thủ công dưới lòng ao - Bán kính rộng hơn rìa ao
        ClearGrassGameObjects(transform.position, pondRadius + 1.2f);

        BuildPond();
        if (spawnLotus) SpawnLotus();
        if (fishCount > 0) SpawnFish();
        if (enableRipples) AddRipples();
        
        Debug.Log($"[FishPond] Đã dựng Hồ Cá Nổi thành công tại vị trí: {transform.position} | Bán kính: {pondRadius} | Số hoa sen: {lotusCount}");
    }

    private void Update()
    {
        BlockPlayerFromEntering();
    }

    private void BlockPlayerFromEntering()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;
        Vector3 pPos = player.transform.position;
        Vector3 pCent = transform.position;
        
        // Đo khoảng cách 2D trên mặt phẳng XZ
        Vector2 diff = new Vector2(pPos.x - pCent.x, pPos.z - pCent.z);
        float dist = diff.magnitude;
        
        // Ngăn chặn người chơi bước chân vào phần lòng hồ (pondRadius). Bờ ngoài thoải bắt đầu từ pondRadius + 0.9f
        float minObstacleRadius = pondRadius;
        
        if (dist < minObstacleRadius)
        {
            Vector2 pushDir = diff.normalized;
            if (pushDir == Vector2.zero) pushDir = Vector2.up;
            Vector3 newPos = new Vector3(
                pCent.x + pushDir.x * minObstacleRadius,
                pPos.y,
                pCent.z + pushDir.y * minObstacleRadius
            );
            player.transform.position = newPos;
        }
    }

    // ── Xây hồ nổi ───────────────────────────────────────────────────────────
    private void BuildPond()
    {
        int segs = 28;

        // ── 1. Bờ đất thoải (Raised Bank) dốc từ đỉnh hồ xuống chạm đất ──
        GameObject bank = new GameObject("Bank_Raised");
        bank.transform.SetParent(transform, false);
        var bmf = bank.AddComponent<MeshFilter>();
        var bmr = bank.AddComponent<MeshRenderer>();
        bmf.sharedMesh     = CreateRaisedBank(pondRadius, pondRadius + 0.9f, pondDepth, segs);
        bmr.sharedMaterial = ProceduralMeshHelper.CreateMaterial(bankColor);

        // Tạo collider theo bờ hồ nổi để nhân vật có thể đi trèo lên/va chạm
        bank.AddComponent<MeshCollider>().sharedMesh = bmf.sharedMesh;

        // ── 2. Đáy lòng hồ (nâng lên trên mặt đất để không bị địa hình đè) ──
        GameObject bottom = new GameObject("Bottom_Raised");
        bottom.transform.SetParent(transform, false);
        bottom.transform.localPosition = new Vector3(0, 0.05f, 0); // Nhô cao hơn mặt cỏ một xíu
        var bof = bottom.AddComponent<MeshFilter>();
        var bor = bottom.AddComponent<MeshRenderer>();
        bof.sharedMesh     = ProceduralMeshHelper.CreateDisk(pondRadius, segs);
        bor.sharedMaterial = ProceduralMeshHelper.CreateMaterial(bottomColor);

        // ── 3. Thành lòng hồ hướng vào trong (Inward Cylinder) ──
        GameObject wall = new GameObject("Wall_Inward");
        wall.transform.SetParent(transform, false);
        wall.transform.localPosition = new Vector3(0, 0.05f, 0);
        var wf = wall.AddComponent<MeshFilter>();
        var wr = wall.AddComponent<MeshRenderer>();
        wf.sharedMesh     = CreateInwardCylinder(pondRadius, pondDepth - 0.05f, segs);
        wr.sharedMaterial = ProceduralMeshHelper.CreateMaterial(bankColor);

        // Tạo collider thành trong để cản người chơi nhảy/đi xuống nước hồ
        wall.AddComponent<MeshCollider>().sharedMesh = wf.sharedMesh;

        // ── 4. Mặt nước hồ (Raised Water) — nằm trên miệng hồ nổi ──
        GameObject water = new GameObject("Water_Raised");
        water.transform.SetParent(transform, false);
        water.transform.localPosition = new Vector3(0, pondDepth - 0.04f, 0); // Sát miệng hồ
        var wamf = water.AddComponent<MeshFilter>();
        var wamr = water.AddComponent<MeshRenderer>();
        wamf.sharedMesh = ProceduralMeshHelper.CreateDisk(pondRadius * 0.99f, segs);

        // Sử dụng helper thông minh tự động nhận diện URP/Standard để bật Alpha Blend mượt mà
        wamr.sharedMaterial = ProceduralMeshHelper.CreateWaterMaterial(waterColor);

        // ── 5. Lớp hào quang / Vòng sáng trên mặt nước ──
        GameObject waterGlow = new GameObject("WaterHighlight");
        waterGlow.transform.SetParent(transform, false);
        waterGlow.transform.localPosition = new Vector3(0, pondDepth - 0.03f, 0); // Tránh z-fighting
        var wgmf = waterGlow.AddComponent<MeshFilter>();
        var wgmr = waterGlow.AddComponent<MeshRenderer>();
        wgmf.sharedMesh = CreateRing(0f, pondRadius * 0.45f, 0f, segs);
        wgmr.sharedMaterial = ProceduralMeshHelper.CreateWaterMaterial(new Color(0.28f, 0.65f, 0.85f, 0.28f));

        // ── 6. Chặn người chơi bước vào lòng hồ bằng CapsuleCollider khổng lồ (Bức tường vô hình) ──
        // Thiết kế hình trụ cao hẳn 10 mét đi thẳng lên trời để CharacterController không thể "auto-step" vượt qua
        var cc = gameObject.AddComponent<CapsuleCollider>();
        cc.center = new Vector3(0f, 5f, 0f); // Tâm ở độ cao 5m
        cc.radius = pondRadius * 0.9f;       // Bán kính cản nước
        cc.height = 10f;                     // Cao 10m cản tuyệt đối mọi hành động trèo/nhảy vào
    }

    // ── Cá chép vàng/đỏ bơi lội ───────────────────────────────────────────────
    private void SpawnFish()
    {
        Mesh fishMesh = CreateFishMesh(fishSize);

        for (int i = 0; i < fishCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float r     = Random.Range(0.3f, pondRadius * 0.7f);
            // Cá bơi lửng lơ giữa đáy (0.1f) và mặt nước (pondDepth - 0.1f)
            float fishY = Random.Range(0.12f, pondDepth - 0.12f);
            Vector3 localPos = new Vector3(Mathf.Cos(angle) * r, fishY, Mathf.Sin(angle) * r);

            GameObject fishGO = new GameObject($"Fish_{i}");
            fishGO.transform.SetParent(transform, false);
            fishGO.transform.localPosition = localPos;
            fishGO.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            var fmf = fishGO.AddComponent<MeshFilter>();
            var fmr = fishGO.AddComponent<MeshRenderer>();
            fmf.sharedMesh     = fishMesh;
            fmr.sharedMaterial = ProceduralMeshHelper.CreateMaterial(
                Random.value > 0.4f ? fishColorA : fishColorB);

            // Gắn controller để cá bơi lội tuần hoàn
            var ctrl = fishGO.AddComponent<FishController>();
            ctrl.pondRadius  = pondRadius * 0.7f;
            // pondCenter ở thế giới (world space) để FishController tính toán chuẩn
            ctrl.pondCenter  = transform.position + new Vector3(0, pondDepth * 0.5f, 0);
            ctrl.swimSpeed   = Random.Range(0.4f, 1.0f);
            ctrl.wiggleAmp   = Random.Range(10f, 18f);
            ctrl.wiggleSpeed = Random.Range(3.5f, 6.5f);
        }
    }

    // ── Hoa sen tụ về lòng hồ nổi ────────────────────────────────────────────
    private void SpawnLotus()
    {
        for (int i = 0; i < lotusCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            // Phân bổ tụ từ tâm ra mép hồ một cách tự nhiên
            float r     = pondRadius * Random.Range(0.3f, 0.82f);
            // Nằm ngay trên mặt nước nổi (pondDepth - 0.035f để chống z-fighting)
            Vector3 localPos = new Vector3(Mathf.Cos(angle) * r, pondDepth - 0.035f, Mathf.Sin(angle) * r);

            GameObject lotusGO = new GameObject($"Lotus_{i}");
            lotusGO.transform.SetParent(transform, false);
            lotusGO.transform.localPosition = localPos;
            lotusGO.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            lotusGO.transform.localScale = Vector3.one * Random.Range(0.6f, 1.1f);
            
            var lotus = lotusGO.AddComponent<LotusFlower>();
            // Tô màu hồng phấn/trắng xen kẽ
            lotus.petalColor = Random.value > 0.4f ? 
                new Color(0.98f, 0.52f, 0.70f) : // Hồng sen
                new Color(0.98f, 0.95f, 0.95f);  // Trắng ngà
        }
    }

    // ── Gợn sóng nước lan tỏa ────────────────────────────────────────────────
    private void AddRipples()
    {
        GameObject rippleGO = new GameObject("Ripples");
        rippleGO.transform.SetParent(transform, false);
        rippleGO.transform.localPosition = new Vector3(0, pondDepth - 0.03f, 0);

        var ps = rippleGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize      = new ParticleSystem.MinMaxCurve(0.18f, 0.55f);
        main.startLifetime  = new ParticleSystem.MinMaxCurve(2.2f, 4f);
        main.startSpeed     = 0;
        main.startColor     = new Color(0.65f, 0.92f, 1f, 0.45f);
        main.maxParticles   = 16;
        main.simulationSpace= ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 1.6f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = pondRadius * 0.6f;
        shape.rotation  = new Vector3(-90f, 0, 0);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0,0), new Keyframe(0.2f,1), new Keyframe(1,2.8f)));

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(0.5f, 0), new GradientAlphaKey(0f, 1) });
        col.color = g;

        ps.Play();

        // Gán chất liệu trong suốt cho bộ sinh hạt để tránh gợn nước bị lỗi hiển thị hình vuông hồng
        var psr = rippleGO.GetComponent<ParticleSystemRenderer>();
        if (psr != null)
        {
            psr.sharedMaterial = ProceduralMeshHelper.CreateWaterMaterial(new Color(0.7f, 0.92f, 1f, 0.22f));
        }
    }

    // ── Tạo Mesh cá 3D cách điệu ─────────────────────────────────────────────
    private static Mesh CreateFishMesh(float size)
    {
        float l = size, w = size * 0.38f, h = size * 0.12f;
        Vector3[] v = {
            new(0, 0, l*0.5f),
            new(w, 0, 0), new(0, h, 0), new(-w, 0, 0), new(0,-h, 0),
            new(0, 0, -l*0.32f),
            new(w*0.9f, 0, -l*0.72f), new(-w*0.9f, 0, -l*0.72f),
        };
        int[] t = {
            0,1,2,  0,2,3,  0,3,4,  0,4,1,
            5,2,1,  5,3,2,  5,4,3,  5,1,4,
            5,6,7,
        };
        var m = new Mesh { vertices = v, triangles = t };
        m.RecalculateNormals();
        return m;
    }

    // ── Mesh bờ thoải nổi dốc xuống mặt đất ─────────────────────────────────
    private static Mesh CreateRaisedBank(float innerR, float outerR, float height, int segs)
    {
        int vc = segs * 4;
        Vector3[] v = new Vector3[vc];
        int[] t     = new int[segs * 6];

        for (int i = 0; i < segs; i++)
        {
            float a0 = Mathf.PI * 2f * i / segs;
            float a1 = Mathf.PI * 2f * (i + 1) / segs;
            
            // Đỉnh bờ (phía trong, nhô cao bằng height)
            v[i*4+0] = new Vector3(Mathf.Cos(a0)*innerR, height, Mathf.Sin(a0)*innerR);
            v[i*4+1] = new Vector3(Mathf.Cos(a1)*innerR, height, Mathf.Sin(a1)*innerR);
            
            // Chân bờ (phía ngoài thoải chạm sát đất Y = 0f)
            v[i*4+2] = new Vector3(Mathf.Cos(a0)*outerR, 0f,     Mathf.Sin(a0)*outerR);
            v[i*4+3] = new Vector3(Mathf.Cos(a1)*outerR, 0f,     Mathf.Sin(a1)*outerR);

            int b = i * 6, vi = i * 4;
            // Đổi thứ tự đỉnh sang Clockwise hướng ra ngoài/lên trên
            t[b+0]=vi;   t[b+1]=vi+3; t[b+2]=vi+2;
            t[b+3]=vi;   t[b+4]=vi+1; t[b+5]=vi+3;
        }
        var m = new Mesh { vertices = v, triangles = t };
        m.RecalculateNormals();
        return m;
    }

    // ── Mesh thành trong của lòng hồ hướng vào trong ────────────────────────
    private static Mesh CreateInwardCylinder(float radius, float height, int segments)
    {
        Mesh mesh = new Mesh { name = "InwardCylinder" };
        int vertCount = (segments + 1) * 2;
        Vector3[] verts = new Vector3[vertCount];
        Vector2[] uvs   = new Vector2[vertCount];

        for (int i = 0; i <= segments; i++)
        {
            float a = Mathf.PI * 2f * i / segments;
            verts[i] = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
            uvs[i]   = new Vector2((float)i / segments, 0);
        }
        int top = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float a = Mathf.PI * 2f * i / segments;
            verts[top + i] = new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius);
            uvs[top + i]   = new Vector2((float)i / segments, 1);
        }

        int[] tris = new int[segments * 6];
        int t = 0;
        for (int i = 0; i < segments; i++)
        {
            // Đảo index winding order để mặt nghiêng hướng vào tâm lòng hồ
            tris[t++] = i;           tris[t++] = top + i;       tris[t++] = i + 1;
            tris[t++] = i + 1;       tris[t++] = top + i;       tris[t++] = top + i + 1;
        }

        mesh.vertices  = verts;
        mesh.uv        = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    // ── Ring mesh phụ trợ ─────────────────────────────────────────────────────
    private static Mesh CreateRing(float innerR, float outerR, float height, int segs)
    {
        if (innerR <= 0f)
            return ProceduralMeshHelper.CreateDisk(outerR, segs, height);

        int vc = segs * 4;
        Vector3[] v = new Vector3[vc];
        int[] t     = new int[segs * 6];

        for (int i = 0; i < segs; i++)
        {
            float a0 = Mathf.PI * 2f * i / segs;
            float a1 = Mathf.PI * 2f * (i + 1) / segs;
            v[i*4+0] = new Vector3(Mathf.Cos(a0)*innerR, 0,      Mathf.Sin(a0)*innerR);
            v[i*4+1] = new Vector3(Mathf.Cos(a1)*innerR, 0,      Mathf.Sin(a1)*innerR);
            v[i*4+2] = new Vector3(Mathf.Cos(a0)*outerR, height, Mathf.Sin(a0)*outerR);
            v[i*4+3] = new Vector3(Mathf.Cos(a1)*outerR, height, Mathf.Sin(a1)*outerR);

            int b = i * 6, vi = i * 4;
            t[b+0]=vi;   t[b+1]=vi+2; t[b+2]=vi+3;
            t[b+3]=vi;   t[b+4]=vi+3; t[b+5]=vi+1;
        }
        var m = new Mesh { vertices = v, triangles = t };
        m.RecalculateNormals();
        return m;
    }

    // ── Tự động xoá cỏ trên Terrain dưới lòng hồ để tránh xuyên mặt nước ──
    private void ClearTerrainDetails(Vector3 center, float radius)
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return;

        TerrainData tData = terrain.terrainData;
        
        // 1. Chuyển đổi vị trí thế giới sang tỉ lệ (0 -> 1) trên Terrain
        Vector3 localCenter = center - terrain.transform.position;
        float normX = localCenter.x / tData.size.x;
        float normZ = localCenter.z / tData.size.z;
        
        // 2. Lấy kích thước bản đồ chi tiết của Terrain
        int mapW = tData.detailWidth;
        int mapH = tData.detailHeight;
        
        // 3. Quy đổi tâm sang toạ độ pixel bản đồ chi tiết
        int centerX = Mathf.RoundToInt(normX * mapW);
        int centerZ = Mathf.RoundToInt(normZ * mapH);
        
        // 4. Tính toán bán kính vùng xoá cỏ (bằng pixels)
        int radX = Mathf.RoundToInt((radius / tData.size.x) * mapW);
        int radZ = Mathf.RoundToInt((radius / tData.size.z) * mapH);
        
        // 5. Thiết lập khoảng đọc an toàn tránh lỗi tràn mảng index out of bounds
        int xStart = Mathf.Max(0, centerX - radX);
        int zStart = Mathf.Max(0, centerZ - radZ);
        int width  = Mathf.Min(mapW - xStart, radX * 2);
        int height = Mathf.Min(mapH - zStart, radZ * 2);
        
        if (width <= 0 || height <= 0) return;

        // 6. Quét qua tất cả các layers cỏ chi tiết
        for (int layer = 0; layer < tData.detailPrototypes.Length; layer++)
        {
            int[,] map = tData.GetDetailLayer(xStart, zStart, width, height, layer);
            
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    float realX = xStart + x;
                    float realZ = zStart + z;
                    
                    float dx = realX - centerX;
                    float dz = realZ - centerZ;
                    
                    // Nếu nằm trong phạm vi hình tròn hồ thì xoá sạch cỏ (độ phủ nhân thêm 1.15 để cỏ rìa sạch)
                    if ((dx * dx) / (radX * radX) + (dz * dz) / (radZ * radZ) <= 1.25f)
                    {
                        map[z, x] = 0;
                    }
                }
            }
            tData.SetDetailLayer(xStart, zStart, layer, map);
        }

        // Bắt buộc Unity nạp lại dữ liệu đồ hoạ và vẽ lại (Flush) cỏ địa hình ngay lập tức!
        terrain.Flush();
    }

    // ── Tự động tìm và huỷ toàn bộ các cỏ cây dạng GameObject/Prefab dưới lòng hồ ──
    private void ClearGrassGameObjects(Vector3 center, float radius)
    {
        // Quét tìm tất cả MeshRenderer của cỏ/cây bụi/cây trong bán kính hồ và huỷ chúng
        var allRenderers = FindObjectsOfType<MeshRenderer>();
        foreach (var r in allRenderers)
        {
            if (r == null) continue;
            Transform rootTransform = r.transform.root;
            GameObject rootGO = rootTransform.gameObject;
            string rootName = rootGO.name.ToLower();
            string selfName = r.gameObject.name.ToLower();
            
            // Lọc các từ khoá liên quan đến cỏ dại trong dự án (bao gồm tiếng Anh, tiếng Tây Ban Nha và tiếng Việt)
            if (rootName.Contains("grass") || rootName.Contains("hierba") || rootName.Contains("weed") || 
                rootName.Contains("blade") || rootName.Contains("plant") || rootName.Contains("low") || 
                rootName.Contains("arada") || rootName.Contains("co") || rootName.Contains("lua") ||
                rootName.Contains("shrub") || rootName.Contains("bush") || rootName.Contains("flora") ||
                rootName.Contains("foliage") || rootName.Contains("vegetation") ||
                selfName.Contains("grass") || selfName.Contains("hierba") || selfName.Contains("weed") || 
                selfName.Contains("blade") || selfName.Contains("plant") || selfName.Contains("shrub") ||
                selfName.Contains("bush") || selfName.Contains("vegetation"))
            {
                // Tính khoảng cách 2D trên mặt phẳng XZ bằng toạ độ gốc (Root) của Prefab cỏ dại
                Vector2 pos2D = new Vector2(rootTransform.position.x, rootTransform.position.z);
                Vector2 center2D = new Vector2(center.x, center.z);
                float dist = Vector2.Distance(pos2D, center2D);
                
                if (dist <= radius)
                {
                    // Huỷ TOÀN BỘ GameObject gốc (Root Prefab) của cỏ dại để tránh bị lỗi hiển thị LOD khác hoặc chỉ huỷ một phần
                    Destroy(rootGO);
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.1f, 0.5f, 0.8f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, pondRadius);
        Gizmos.color = new Color(0.4f, 0.3f, 0.1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, pondRadius + 0.9f);
    }
#endif
}
