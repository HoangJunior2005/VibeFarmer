using UnityEngine;

/// <summary>
/// Utility: tạo các mesh cơ bản dùng chung cho toàn bộ hệ thống môi trường làng quê.
/// Đã sửa đổi toàn bộ Winding Order để đảm bảo các mặt hiển thị đúng chiều (Clockwise),
/// tránh bị triệt tiêu mặt (Backface Culling) trong Unity.
/// </summary>
public static class ProceduralMeshHelper
{
    // ── Cylinder (Thân cây, cọc - Hướng mặt ra ngoài chuẩn) ──────────────────────────
    public static Mesh CreateCylinder(float radius, float height, int segments = 12)
    {
        Mesh mesh = new Mesh { name = "Cylinder" };
        int vertCount = (segments + 1) * 2 + segments * 2;
        Vector3[] verts = new Vector3[vertCount];
        Vector2[] uvs  = new Vector2[vertCount];

        // Vòng dưới
        for (int i = 0; i <= segments; i++)
        {
            float a = Mathf.PI * 2f * i / segments;
            verts[i] = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
            uvs[i]   = new Vector2((float)i / segments, 0);
        }
        // Vòng trên
        int top = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float a = Mathf.PI * 2f * i / segments;
            verts[top + i] = new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius);
            uvs[top + i]   = new Vector2((float)i / segments, 1);
        }

        // Đáy và nắp
        int capBase = top + segments + 1;
        for (int i = 0; i < segments; i++)
        {
            float a = Mathf.PI * 2f * i / segments;
            verts[capBase + i]           = new Vector3(Mathf.Cos(a) * radius, 0,      Mathf.Sin(a) * radius);
            verts[capBase + segments + i] = new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius);
            uvs[capBase + i]              = new Vector2(Mathf.Cos(a) * .5f + .5f, Mathf.Sin(a) * .5f + .5f);
            uvs[capBase + segments + i]   = uvs[capBase + i];
        }

        int triCount = segments * 6 + (segments - 2) * 3 * 2;
        int[] tris = new int[triCount];
        int t = 0;
        
        // Thân (sửa lại để hướng ra ngoài - Clockwise)
        for (int i = 0; i < segments; i++)
        {
            tris[t++] = i;           tris[t++] = top + i;       tris[t++] = i + 1;
            tris[t++] = i + 1;       tris[t++] = top + i;       tris[t++] = top + i + 1;
        }
        // Đáy (hướng xuống dưới - Clockwise từ dưới nhìn lên)
        for (int i = 1; i < segments - 1; i++)
        {
            tris[t++] = capBase;     tris[t++] = capBase + i;   tris[t++] = capBase + i + 1;
        }
        // Nắp (hướng lên trên - Clockwise từ trên nhìn xuống)
        int cn = capBase + segments;
        for (int i = 1; i < segments - 1; i++)
        {
            tris[t++] = cn;          tris[t++] = cn + i + 1;    tris[t++] = cn + i;
        }

        mesh.vertices  = verts;
        mesh.uv        = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    // ── Sphere (Tán lá tròn) ───────────────────────────────────────────────────
    public static Mesh CreateSphere(float radius, int lat = 10, int lon = 16)
    {
        Mesh mesh = new Mesh { name = "Sphere" };
        var verts = new System.Collections.Generic.List<Vector3>();
        var uvs   = new System.Collections.Generic.List<Vector2>();
        var tris  = new System.Collections.Generic.List<int>();

        for (int i = 0; i <= lat; i++)
        {
            float theta = Mathf.PI * i / lat;
            for (int j = 0; j <= lon; j++)
            {
                float phi = Mathf.PI * 2f * j / lon;
                verts.Add(new Vector3(
                    Mathf.Sin(theta) * Mathf.Cos(phi) * radius,
                    Mathf.Cos(theta) * radius,
                    Mathf.Sin(theta) * Mathf.Sin(phi) * radius));
                uvs.Add(new Vector2((float)j / lon, 1f - (float)i / lat));
            }
        }

        int w = lon + 1;
        for (int i = 0; i < lat; i++)
            for (int j = 0; j < lon; j++)
            {
                int a = i * w + j, b = a + 1, c = a + w, d = c + 1;
                // Thao tác thứ tự đúng chiều Clockwise
                tris.Add(a); tris.Add(b); tris.Add(c);
                tris.Add(b); tris.Add(d); tris.Add(c);
            }

        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        return mesh;
    }

    // ── Cone (Búp sen, đầu cá - Hướng mặt ra ngoài chuẩn) ───────────────────────────
    public static Mesh CreateCone(float baseRadius, float height, int segments = 10)
    {
        Mesh mesh = new Mesh { name = "Cone" };
        Vector3[] verts = new Vector3[segments + 2];
        int[] tris      = new int[segments * 6];

        verts[0] = Vector3.zero; // Tâm đáy
        verts[1] = new Vector3(0, height, 0); // Đỉnh cone
        for (int i = 0; i < segments; i++)
        {
            float a = Mathf.PI * 2f * i / segments;
            verts[i + 2] = new Vector3(Mathf.Cos(a) * baseRadius, 0, Mathf.Sin(a) * baseRadius);
        }

        int t = 0;
        for (int i = 0; i < segments; i++)
        {
            int cur  = i + 2;
            int next = (i + 1) % segments + 2;
            
            // Mặt bên (hướng ra ngoài - Clockwise)
            tris[t++] = 1; tris[t++] = next; tris[t++] = cur;
            // Đáy (hướng xuống dưới - Clockwise từ dưới nhìn lên)
            tris[t++] = 0; tris[t++] = cur; tris[t++] = next;
        }

        mesh.vertices  = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    // ── Disk dẹt (Lá sen, mặt hồ - Hướng mặt lên trên chuẩn, khép kín 100%) ────────────────────────
    public static Mesh CreateDisk(float radius, int segments = 16, float height = 0f)
    {
        Mesh mesh  = new Mesh { name = "Disk" };
        Vector3[] verts = new Vector3[segments + 1];
        Vector2[] uvs   = new Vector2[segments + 1];
        int[] tris      = new int[segments * 3]; // Khép kín toàn bộ vòng tròn

        verts[0] = new Vector3(0, height, 0);
        uvs[0]   = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < segments; i++)
        {
            float a  = Mathf.PI * 2f * i / segments;
            verts[i + 1] = new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius);
            uvs[i + 1]   = new Vector2(Mathf.Cos(a) * 0.5f + 0.5f, Mathf.Sin(a) * 0.5f + 0.5f);
        }

        int t = 0;
        // Chạy qua tất cả các segments và nối điểm cuối trở về điểm bắt đầu (curr -> next)
        for (int i = 0; i < segments; i++)
        {
            int curr = i + 1;
            int next = (i + 1) % segments + 1;

            tris[t++] = 0;
            tris[t++] = next;
            tris[t++] = curr;
        }

        mesh.vertices  = verts;
        mesh.uv        = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    // ── Quad phẳng (Mặt nước hồ - Clockwise) ──────────────────────────────────
    public static Mesh CreateQuad(float width, float depth)
    {
        Mesh mesh = new Mesh { name = "Quad" };
        float hw = width * 0.5f, hd = depth * 0.5f;
        mesh.vertices  = new[] {
            new Vector3(-hw, 0, -hd), new Vector3( hw, 0, -hd),
            new Vector3(-hw, 0,  hd), new Vector3( hw, 0,  hd) };
        mesh.uv        = new[] {
            new Vector2(0,0), new Vector2(1,0), new Vector2(0,1), new Vector2(1,1) };
        // Sửa winding order từ 0,2,1 sang 0,1,2 và 2,1,3 để hướng lên trên
        mesh.triangles = new[] { 0, 1, 2, 2, 1, 3 };
        mesh.RecalculateNormals();
        return mesh;
    }

    // ── Helper: tạo Material hỗ trợ cả URP Lit và Standard ──────────────────
    public static Material CreateMaterial(Color color, bool transparent = false)
    {
        Shader shader = null;
        if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            shader = Shader.Find("Diffuse");
        }

        var mat = new Material(shader);
        mat.color = color;

        if (transparent)
        {
            if (shader.name.Contains("Universal Render Pipeline"))
            {
                mat.SetFloat("_Surface", 1f);          // 1 = Transparent
                mat.SetFloat("_Blend", 0f);             // 0 = Alpha
                mat.SetFloat("_ZWrite", 0f);
                mat.SetFloat("_AlphaClip", 0f);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.renderQueue = 3000;
            }
            else
            {
                mat.SetFloat("_Mode", 3f); // 3 = Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }
        return mat;
    }

    // ── Tạo material đơn giản hơn cho mặt nước (hỗ trợ cả URP Unlit và Unlit/Color) ────────
    public static Material CreateWaterMaterial(Color color)
    {
        Shader shader = null;
        if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        var mat = new Material(shader);
        mat.color = color;

        if (shader.name.Contains("Universal Render Pipeline"))
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3000;
        }
        else
        {
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
        }
        return mat;
    }
}
