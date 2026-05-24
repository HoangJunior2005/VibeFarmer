using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Tự sinh toàn bộ UI Main Menu Thôn An Lúa khi chạy.
/// Chỉ cần gắn script này vào một GameObject rỗng trong Scene Main.
/// </summary>
public class MainMenuBuilder : MonoBehaviour
{
    [Header("=== SCENE GAMEPLAY ===")]
    public string gameSceneName = "TownScene";

    // ─── Palette màu truyền thống & Làng quê Việt Nam ───────────────────────
    static readonly Color C_DO_SON      = new Color(0.72f, 0.08f, 0.04f, 1.00f); // Đỏ son
    static readonly Color C_DO_NHAT     = new Color(0.55f, 0.06f, 0.03f, 0.90f); // Đỏ tối hơn
    static readonly Color C_VANG_DONG   = new Color(0.88f, 0.68f, 0.12f, 1.00f); // Vàng đồng
    static readonly Color C_VANG_SANG   = new Color(1.00f, 0.92f, 0.55f, 1.00f); // Vàng sáng
    static readonly Color C_NAU_DAM     = new Color(0.10f, 0.04f, 0.01f, 0.96f); // Nâu gỗ đậm
    static readonly Color C_NAU_VIEN    = new Color(0.18f, 0.07f, 0.02f, 0.98f); // Nâu viền
    static readonly Color C_KEM         = new Color(0.97f, 0.93f, 0.80f, 1.00f); // Kem trắng
    static readonly Color C_OVERLAY     = new Color(0.05f, 0.02f, 0.00f, 0.20f); // Overlay nhẹ

    // Tông màu làng quê Việt Nam mới (Rural Palette)
    static readonly Color C_WOOD_DARK   = new Color(0.24f, 0.12f, 0.05f, 1.00f); // Nâu gỗ đậm (khung treo & viền)
    static readonly Color C_WOOD_LIGHT  = new Color(0.55f, 0.27f, 0.07f, 1.00f); // Nâu gỗ ấm (nút bấm)
    static readonly Color C_PAPER       = new Color(0.97f, 0.93f, 0.82f, 1.00f); // Giấy kem ấm
    static readonly Color C_ROPE        = new Color(0.68f, 0.55f, 0.38f, 1.00f); // Dây thừng tan
    static readonly Color C_BAMBOO_DK   = new Color(0.12f, 0.35f, 0.12f, 1.00f); // Xanh tre sẫm
    static readonly Color C_BAMBOO_LT   = new Color(0.25f, 0.58f, 0.18f, 1.00f); // Xanh tre tươi
    static readonly Color C_RICE_GOLD   = new Color(0.92f, 0.72f, 0.15f, 1.00f); // Hạt lúa chín vàng
    static readonly Color C_RICE_ORG    = new Color(0.85f, 0.55f, 0.10f, 1.00f); // Hạt lúa cam ấm

    // ─── Font chữ mộc mạc ───────────────────────────────────────────────────
    static Font F_TITLE; // Font Gabriola viết thư pháp
    static Font F_BODY;  // Font Georgia viết chữ nghiên cứu/thông số

    void Awake() => BuildUI();

    // ════════════════════════════════════════════════════════════════════════
    void LoadFonts()
    {
        F_TITLE = Resources.Load<Font>("Gabriola");
        if (F_TITLE == null)
        {
            F_TITLE = Font.CreateDynamicFontFromOSFont("Gabriola", 40);
        }

        F_BODY = Resources.Load<Font>("Georgia");
        if (F_BODY == null)
        {
            F_BODY = Font.CreateDynamicFontFromOSFont("Georgia", 24);
        }

        // Fallback nếu không load được
        var fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (F_TITLE == null) F_TITLE = fallback;
        if (F_BODY  == null) F_BODY  = fallback;
    }

    void BuildUI()
    {
        // ── Nạp Font chữ ────────────────────────────────────────────────────
        LoadFonts();

        // ── EventSystem (classic Input System) ───────────────────────────────
        if (Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        // ── Canvas ──────────────────────────────────────────────────────────
        var canvasGO = new GameObject("MainMenuCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode    = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder  = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Background ──────────────────────────────────────────────────────
        BuildBackground(canvasGO);

        // ── Floating particles behind UI but in front of background ─────────
        BuildFloatingParticles(canvasGO);

        // ── Panels ──────────────────────────────────────────────────────────
        var mainPanel    = BuildMainPanel(canvasGO);
        var optionsPanel = BuildOptionsPanel(canvasGO);
        var exitPanel    = BuildExitPanel(canvasGO);

        optionsPanel.SetActive(false);
        exitPanel.SetActive(false);

        // ── Fade overlay ────────────────────────────────────────────────────
        var fadeGroup = MakeFadeOverlay(canvasGO);

        // ── Gắn Manager ─────────────────────────────────────────────────────
        var mgr = gameObject.AddComponent<MainMenuManager>();
        mgr.mainMenuPanel    = mainPanel;
        mgr.optionsPanel     = optionsPanel;
        mgr.confirmExitPanel = exitPanel;
        mgr.gameSceneName    = gameSceneName;
        mgr.fadeDuration     = 0.7f;
        mgr.fadeOverlay      = fadeGroup;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  FLOATING PARTICLES BACKGROUND
    // ════════════════════════════════════════════════════════════════════════
    void BuildFloatingParticles(GameObject parent)
    {
        var container = UI("FloatingParticles", parent);
        Stretch(container);
        
        // Spawn 8 floating leaves
        for (int i = 0; i < 8; i++)
        {
            var leaf = UI("FloatingLeaf_" + i, container);
            var rt = leaf.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40f, 10f);
            
            // Random initial position
            float px = Random.Range(100f, 1800f);
            float py = Random.Range(100f, 980f);
            rt.anchoredPosition = new Vector2(px, py);
            
            var img = leaf.AddComponent<Image>();
            Color leafColor = Color.Lerp(C_BAMBOO_DK, C_BAMBOO_LT, Random.value);
            leafColor.a = Random.Range(0.4f, 0.7f); // subtle transparency
            img.sprite = MakeRoundedSprite(80, 20, 10, leafColor);
            img.type = Image.Type.Sliced;
            
            var fl = leaf.AddComponent<FloatingParticle>();
            fl.speed = Random.Range(30f, 80f);
            fl.rotSpeed = Random.Range(-40f, 40f);
            fl.windFrequency = Random.Range(0.5f, 1.5f);
            fl.windAmplitude = Random.Range(15f, 35f);
            fl.isLeaf = true;
        }

        // Spawn 8 floating golden rice grains
        for (int i = 0; i < 8; i++)
        {
            var grain = UI("FloatingGrain_" + i, container);
            var rt = grain.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(16f, 8f);
            
            // Random initial position
            float px = Random.Range(100f, 1800f);
            float py = Random.Range(100f, 980f);
            rt.anchoredPosition = new Vector2(px, py);
            
            var img = grain.AddComponent<Image>();
            Color grainColor = Color.Lerp(C_RICE_GOLD, C_RICE_ORG, Random.value);
            grainColor.a = Random.Range(0.3f, 0.6f);
            img.sprite = MakeRoundedSprite(32, 16, 8, grainColor);
            img.type = Image.Type.Sliced;
            
            var fl = grain.AddComponent<FloatingParticle>();
            fl.speed = Random.Range(20f, 50f);
            fl.rotSpeed = Random.Range(-25f, 25f);
            fl.windFrequency = Random.Range(0.4f, 1.2f);
            fl.windAmplitude = Random.Range(10f, 25f);
            fl.isLeaf = false;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  BACKGROUND
    // ════════════════════════════════════════════════════════════════════════
    void BuildBackground(GameObject parent)
    {
        // Ảnh nền
        var bg    = UI("BG", parent);  Stretch(bg);
        var bgImg = bg.AddComponent<Image>();
        var bgTex = Resources.Load<Texture2D>("VietnamVillageBG");
        if (bgTex != null)
        {
            bgImg.sprite = Sprite.Create(bgTex,
                new Rect(0,0,bgTex.width,bgTex.height), Vector2.one*0.5f, 100f,
                0, SpriteMeshType.FullRect);
        }
        else
            bgImg.color = new Color(0.09f, 0.22f, 0.06f);

        bgImg.type = Image.Type.Simple;
        bgImg.preserveAspect = false;

        // Lớp phủ gradient dưới — nhẹ hơn để nền rõ
        var grad = UI("Grad", parent);
        Anchor(grad, 0,0,1,0.45f);
        grad.AddComponent<Image>().color = new Color(0.04f,0.01f,0f,0.45f);

        // Overlay tổng thể rất nhẹ
        var ov = UI("Overlay", parent); Stretch(ov);
        ov.AddComponent<Image>().color = C_OVERLAY;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  MAIN MENU PANEL
    // ════════════════════════════════════════════════════════════════════════
    GameObject BuildMainPanel(GameObject parent)
    {
        var panel = UI("MainPanel", parent); Stretch(panel);

        // ── KHU VỰC LOGO BÊN TRÁI ──────────────────────────────────────────
        var logoContainer = UI("LogoContainer", panel);
        Anchor(logoContainer, 0.08f, 0.15f, 0.48f, 0.85f);
        logoContainer.AddComponent<SlowHover>();

        // Sinh vòng nguyệt quế lá tre & bông lúa đằng sau logo
        var wreath = UI("Wreath", logoContainer); Stretch(wreath);
        
        // Sinh lá tre (được làm to hơn và vươn ra ngoài để ôm lấy logo tròn lớn)
        int leafCount = 32;
        for (int i = 0; i < leafCount; i++)
        {
            float angle = (i * 360f / leafCount) * Mathf.Deg2Rad;
            float rx = 290f; // Tăng bán kính X để chui hẳn ra ngoài logo 500px
            float ry = 290f; // Tăng bán kính Y
            float px = Mathf.Cos(angle) * rx;
            float py = Mathf.Sin(angle) * ry;

            var leaf = UI("Leaf_" + i, wreath);
            var rt = leaf.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 26f); // Lá to hơn, sum suê hơn
            rt.anchoredPosition = new Vector2(px, py);
            rt.localRotation = Quaternion.Euler(0f, 0f, (i * 360f / leafCount) + 90f + Random.Range(-10f, 10f));

            var img = leaf.AddComponent<Image>();
            Color leafColor = Color.Lerp(C_BAMBOO_DK, C_BAMBOO_LT, Random.value);
            img.sprite = MakeRoundedSprite(160, 26, 13, leafColor);
            img.type = Image.Type.Sliced;
        }

        // Sinh cành lúa chín trĩu hạt (uốn cong ôm lấy vành dưới của logo)
        for (int side = -1; side <= 1; side += 2)
        {
            var stalk = UI("Stalk_" + (side > 0 ? "R" : "L"), wreath);
            int grainCount = 10;
            for (int g = 0; g < grainCount; g++)
            {
                float t = (float)g / (grainCount - 1);
                float angle = (-65f + side * 45f + t * 85f * side) * Mathf.Deg2Rad;
                float rx = 300f + t * 40f; // Bán kính lúa lớn hơn
                float ry = -200f - t * 80f; // Đẩy lúa xuống vành dưới tròn trịa
                float px = Mathf.Cos(angle) * rx;
                float py = Mathf.Sin(angle) * ry;

                var grain = UI("Grain_" + g, stalk);
                var rt = grain.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(28f, 14f); // Hạt lúa to hơn chín căng tròn
                rt.anchoredPosition = new Vector2(px, py);
                rt.localRotation = Quaternion.Euler(0f, 0f, (angle * Mathf.Rad2Deg) + 90f + (g * 8f * side));

                var img = grain.AddComponent<Image>();
                Color grainColor = Color.Lerp(C_RICE_GOLD, C_RICE_ORG, Random.value);
                img.sprite = MakeRoundedSprite(56, 28, 14, grainColor);
                img.type = Image.Type.Sliced;
            }
        }

        // Đặt Logo game đè lên trên vòng nguyệt quế
        var logoImg = UI("LogoImg", logoContainer); Stretch(logoImg);
        var logoI = logoImg.AddComponent<Image>();
        var logoTex = Resources.Load<Texture2D>("GameTitleLogo");
        if (logoTex != null)
        {
            logoTex.filterMode = FilterMode.Trilinear;
            logoTex.anisoLevel = 16;
            logoI.sprite = Sprite.Create(logoTex,
                new Rect(0, 0, logoTex.width, logoTex.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            logoI.preserveAspect = true;
            logoI.color = Color.white;
        }
        else
        {
            var ft = UI("FallbackTitle", logoContainer); Stretch(ft);
            MakeText(ft, "THÔN AN LÚA", 72, FontStyle.Bold, C_RICE_GOLD, TextAnchor.MiddleCenter, F_TITLE);
            AddOutline(ft, C_WOOD_DARK, 3f);
        }

        // ── KHU VỰC BẢNG TREO BÊN PHẢI ──────────────────────────────────────
        // Dây treo bên trái dạng tết thừng dừa
        var ropeL = UI("RopeL", panel);
        Anchor(ropeL, 0.628f, 0.78f, 0.634f, 1.00f);
        BuildBraidedRope(ropeL);

        // Dây treo bên phải dạng tết thừng dừa
        var ropeR = UI("RopeR", panel);
        Anchor(ropeR, 0.825f, 0.78f, 0.831f, 1.00f);
        BuildBraidedRope(ropeR);

        // Lá tre rủ rậm rạp trên trần dây treo trái (chống trống trải)
        var topDecorL = UI("TopDecorL", panel);
        Anchor(topDecorL, 0.60f, 0.90f, 0.66f, 0.97f); // Điều chỉnh hạ thấp hơn một chút để rủ đẹp mắt
        for (int i = 0; i < 4; i++)
        {
            var leaf = UI("Leaf_" + i, topDecorL);
            var rt = leaf.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(70f, 16f);
            rt.anchoredPosition = new Vector2(i * 12f, -i * 8f);
            rt.localRotation = Quaternion.Euler(0f, 0f, -30f + i * 18f);
            var img = leaf.AddComponent<Image>();
            img.sprite = MakeRoundedSprite(140, 32, 16, C_BAMBOO_LT);
            img.type = Image.Type.Sliced;
        }

        // Lá tre rủ rậm rạp trên trần dây treo phải
        var topDecorR = UI("TopDecorR", panel);
        Anchor(topDecorR, 0.80f, 0.90f, 0.86f, 0.97f);
        for (int i = 0; i < 4; i++)
        {
            var leaf = UI("Leaf_" + i, topDecorR);
            var rt = leaf.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(70f, 16f);
            rt.anchoredPosition = new Vector2(-i * 12f, -i * 8f);
            rt.localRotation = Quaternion.Euler(0f, 0f, 30f - i * 18f);
            var img = leaf.AddComponent<Image>();
            img.sprite = MakeRoundedSprite(140, 32, 16, C_BAMBOO_LT);
            img.type = Image.Type.Sliced;
        }

        // Biển gỗ tiêu đề treo lơ lửng
        var titlePlate = UI("TitlePlate", panel);
        Anchor(titlePlate, 0.61f, 0.80f, 0.85f, 0.89f);
        var tpImg = titlePlate.AddComponent<Image>();
        tpImg.sprite = MakeRoundedSprite(320, 80, 16, C_WOOD_DARK);
        tpImg.type = Image.Type.Sliced;
        BuildBorderFull(titlePlate, C_RICE_GOLD, 2.5f);

        // Cành lá tre rủ sang hai bên biển tiêu đề cho đẹp mắt
        var tpLeafL = UI("LeafL", titlePlate);
        Anchor(tpLeafL, 0f, 0.5f, 0f, 0.5f);
        tpLeafL.GetComponent<RectTransform>().anchoredPosition = new Vector2(-15f, 0f);
        var lLeaf = UI("L", tpLeafL);
        lLeaf.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 13f);
        lLeaf.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, 25f);
        lLeaf.AddComponent<Image>().sprite = MakeRoundedSprite(100, 26, 13, C_BAMBOO_LT);

        var tpLeafR = UI("LeafR", titlePlate);
        Anchor(tpLeafR, 1f, 0.5f, 1f, 0.5f);
        tpLeafR.GetComponent<RectTransform>().anchoredPosition = new Vector2(15f, 0f);
        var rLeaf = UI("R", tpLeafR);
        rLeaf.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 13f);
        rLeaf.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, -25f);
        rLeaf.AddComponent<Image>().sprite = MakeRoundedSprite(100, 26, 13, C_BAMBOO_LT);

        var tpText = UI("Text", titlePlate); Stretch(tpText);
        MakeText(tpText, "MENU CHÍNH", 42, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, F_TITLE);
        AddOutline(tpText, C_WOOD_DARK, 2.5f);

        // Vòng thắt dây thừng nối ở đỉnh bảng chính
        var knotL = UI("KnotL", panel);
        Anchor(knotL, 0.622f, 0.765f, 0.643f, 0.795f);
        knotL.AddComponent<Image>().sprite = MakeRoundedSprite(40, 40, 20, C_ROPE);
        BuildBorderFull(knotL, C_WOOD_DARK, 2f);
        BuildTiedKnot(knotL);

        var knotR = UI("KnotR", panel);
        Anchor(knotR, 0.817f, 0.765f, 0.838f, 0.795f);
        knotR.AddComponent<Image>().sprite = MakeRoundedSprite(40, 40, 20, C_ROPE);
        BuildBorderFull(knotR, C_WOOD_DARK, 2f);
        BuildTiedKnot(knotR);

        // Bảng gỗ chính chứa giấy
        var boardContainer = UI("BoardContainer", panel);
        Anchor(boardContainer, 0.56f, 0.06f, 0.90f, 0.78f);

        var frame = UI("Frame", boardContainer); Stretch(frame);
        var frameBg = frame.AddComponent<Image>();
        frameBg.sprite = MakeRoundedSprite(500, 750, 26, C_WOOD_DARK);
        frameBg.type = Image.Type.Sliced;
        BuildBorderFull(frame, C_ROPE, 3.5f);

        // Tờ giấy kem giấy da
        var paper = UI("PaperSheet", frame); Stretch(paper);
        var prt = paper.GetComponent<RectTransform>();
        prt.offsetMin = new Vector2(16, 16);
        prt.offsetMax = new Vector2(-16, -16);
        var paperBg = paper.AddComponent<Image>();
        paperBg.sprite = MakeRoundedSprite(460, 710, 20, new Color(0.22f, 0.11f, 0.04f, 0.18f)); // Viền tối cháy mép giấy da cổ kính
        paperBg.type = Image.Type.Sliced;

        var paperInner = UI("PaperInner", paper); Stretch(paperInner);
        var pit = paperInner.GetComponent<RectTransform>();
        pit.offsetMin = new Vector2(4, 4);
        pit.offsetMax = new Vector2(-4, -4);
        var innerImg = paperInner.AddComponent<Image>();
        innerImg.sprite = MakeRoundedSprite(452, 702, 18, C_PAPER);
        innerImg.type = Image.Type.Sliced;

        // Viền chỉ đôi sang trọng trong giấy da (Inner double border)
        var innerBorder = UI("InnerBorder", paperInner); Stretch(innerBorder);
        var ibt = innerBorder.GetComponent<RectTransform>();
        ibt.offsetMin = new Vector2(12, 12);
        ibt.offsetMax = new Vector2(-12, -12);
        BuildBorderFull(innerBorder, new Color(0.55f, 0.27f, 0.07f, 0.35f), 1.5f);

        // 4 Góc giấy có nẹp gỗ tròn & đinh tán đồng trang nhã
        string[] corners = { "TL", "TR", "BL", "BR" };
        foreach (var c in corners)
        {
            var cgo = UI("Corner_" + c, paperInner);
            var crt = cgo.GetComponent<RectTransform>();
            float x = c.Contains("L") ? 0f : 1f;
            float y = c.Contains("B") ? 0f : 1f;
            crt.anchorMin = new Vector2(x, y);
            crt.anchorMax = new Vector2(x, y);
            crt.sizeDelta = new Vector2(22f, 22f);
            crt.anchoredPosition = new Vector2(c.Contains("L") ? 18f : -18f, c.Contains("B") ? 18f : -18f);
            
            var cimg = cgo.AddComponent<Image>();
            cimg.sprite = MakeRoundedSprite(44, 44, 22, C_WOOD_DARK);
            
            var dot = UI("Dot", cgo);
            var drt = dot.GetComponent<RectTransform>();
            drt.anchorMin = drt.anchorMax = Vector2.one * 0.5f;
            drt.sizeDelta = new Vector2(8f, 8f);
            dot.AddComponent<Image>().sprite = MakeRoundedSprite(16, 16, 8, C_RICE_GOLD);
        }

        // Lời chào mừng mộc mạc điền khoảng trống trên nút Bắt đầu
        var welcomeGO = UI("WelcomeBanner", paperInner);
        Anchor(welcomeGO, 0.05f, 0.81f, 0.95f, 0.92f);
        var welcomeTxt = welcomeGO.AddComponent<Text>();
        welcomeTxt.text = "❖  Huyền Thoại Làng Quê  ❖";
        welcomeTxt.font = F_TITLE;
        welcomeTxt.fontSize = 32;
        welcomeTxt.fontStyle = FontStyle.Bold;
        welcomeTxt.color = C_WOOD_DARK;
        welcomeTxt.alignment = TextAnchor.MiddleCenter;
        AddOutline(welcomeGO, new Color(C_RICE_GOLD.r, C_RICE_GOLD.g, C_RICE_GOLD.b, 0.3f), 1.5f);

        // Trang trí lá tre góc trên-trái giấy kem (được điều chỉnh vị trí xuống dưới welcome banner)
        var decorTL = UI("DecorTL", paperInner);
        Anchor(decorTL, 0f, 0.80f, 0f, 0.80f);
        decorTL.GetComponent<RectTransform>().anchoredPosition = new Vector2(18f, 0f);
        for (int i = 0; i < 3; i++)
        {
            var leaf = UI("Leaf_" + i, decorTL);
            var rt = leaf.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50f, 12f);
            rt.anchoredPosition = new Vector2(i * 10f, -i * 8f);
            rt.localRotation = Quaternion.Euler(0f, 0f, -20f - i * 15f);
            var img = leaf.AddComponent<Image>();
            img.sprite = MakeRoundedSprite(100, 24, 12, C_BAMBOO_LT);
            img.type = Image.Type.Sliced;
        }

        // Trang trí hạt lúa góc dưới-phải giấy kem
        var decorBR = UI("DecorBR", paperInner);
        Anchor(decorBR, 1f, 0.20f, 1f, 0.20f);
        decorBR.GetComponent<RectTransform>().anchoredPosition = new Vector2(-22f, 0f);
        for (int i = 0; i < 4; i++)
        {
            var grain = UI("Grain_" + i, decorBR);
            var rt = grain.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(16f, 8f);
            rt.anchoredPosition = new Vector2(-i * 8f, i * 6f);
            rt.localRotation = Quaternion.Euler(0f, 0f, 40f + i * 15f);
            var img = grain.AddComponent<Image>();
            img.sprite = MakeRoundedSprite(32, 16, 8, C_RICE_GOLD);
            img.type = Image.Type.Sliced;
        }

        // ── 3 Nút Menu đặt trên giấy kem ─────────────────────────────────────
        MakeMainBtn(paperInner, "⚔  BẮT ĐẦU",   "OnClickStart",   0.60f, 0.74f, C_WOOD_LIGHT);
        MakeMainBtn(paperInner, "⚙  TÙY CHỌN",  "OnClickOptions", 0.42f, 0.56f, C_WOOD_LIGHT);
        MakeMainBtn(paperInner, "✖  THOÁT",      "OnClickExit",    0.24f, 0.38f, C_WOOD_LIGHT);

        // Bài thơ ca dao điền khoảng trống bên dưới nút Thoát (Village Folk Poem)
        var poemGO = UI("VillagePoem", paperInner);
        Anchor(poemGO, 0.05f, 0.08f, 0.95f, 0.21f);
        var poemTxt = poemGO.AddComponent<Text>();
        poemTxt.text = "“Ai ơi về với thôn ta,\nĐồng thơm lúa chín, bóng tre hiền hòa.\nKhách quê dừng bước thong thả...”";
        poemTxt.font = F_BODY;
        poemTxt.fontSize = 18;
        poemTxt.fontStyle = FontStyle.Italic;
        poemTxt.color = new Color(C_WOOD_DARK.r, C_WOOD_DARK.g, C_WOOD_DARK.b, 0.70f);
        poemTxt.alignment = TextAnchor.MiddleCenter;
        poemTxt.lineSpacing = 1.3f;

        // Nhãn phiên bản
        var verGO = UI("Version", paperInner);
        Anchor(verGO, 0.05f, 0.02f, 0.95f, 0.06f);
        var verTxt = verGO.AddComponent<Text>();
        verTxt.text      = "Thôn An Lúa  v0.1 Alpha  •  © 2025";
        verTxt.font      = F_BODY;
        verTxt.fontSize  = 12;
        verTxt.fontStyle = FontStyle.Bold;
        verTxt.color     = new Color(C_WOOD_DARK.r, C_WOOD_DARK.g, C_WOOD_DARK.b, 0.60f);
        verTxt.alignment = TextAnchor.MiddleCenter;

        return panel;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  OPTIONS PANEL
    // ════════════════════════════════════════════════════════════════════════
    GameObject BuildOptionsPanel(GameObject parent)
    {
        var panel = UI("OptionsPanel", parent); Stretch(panel);
        var bg = panel.AddComponent<Image>(); bg.color = new Color(0,0,0,0.65f);

        var win = UI("Win", panel);
        Anchor(win, 0.28f, 0.08f, 0.72f, 0.94f);
        win.AddComponent<Image>().color = C_WOOD_DARK; // Nâu gỗ sẫm đồng bộ
        BuildBorderFull(win, C_RICE_GOLD, 3f);

        // Top bar gỗ ấm
        var tb = UI("TopBar", win); Anchor(tb,0,0.93f,1,1f);
        tb.AddComponent<Image>().color = C_WOOD_LIGHT;

        // Tiêu đề
        var t = UI("T", win); Anchor(t, 0.02f,0.82f,0.98f,0.94f);
        MakeText(t, "⚙  TÙY CHỌN", 42, FontStyle.Bold, C_PAPER, TextAnchor.MiddleCenter, F_TITLE);
        HLine(win, 0.81f);

        // Sliders
        MakeSliderRow(win, "🔊  Âm Lượng Tổng",      0.78f);
        MakeSliderRow(win, "🎵  Âm Nhạc",             0.63f);
        MakeSliderRow(win, "💥  Hiệu Ứng Âm Thanh",  0.48f);

        // Fullscreen toggle
        MakeToggleRow(win, "🖥   Toàn Màn Hình", 0.32f);

        HLine(win, 0.20f);

        // Nút quay lại
        MakeSmallBtn(win, "◀  QUAY LẠI", "OnClickBackFromOptions", 0.06f, 0.17f);

        return panel;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  EXIT CONFIRM PANEL
    // ════════════════════════════════════════════════════════════════════════
    GameObject BuildExitPanel(GameObject parent)
    {
        var panel = UI("ExitPanel", parent); Stretch(panel);
        panel.AddComponent<Image>().color = new Color(0,0,0,0.72f);

        var dlg = UI("Dlg", panel);
        Anchor(dlg, 0.31f, 0.35f, 0.69f, 0.67f);
        dlg.AddComponent<Image>().color = C_WOOD_DARK;
        BuildBorderFull(dlg, C_RICE_GOLD, 3f);

        // Tiêu đề
        var tb = UI("H", dlg); Anchor(tb,0,0.85f,1,1f);
        tb.AddComponent<Image>().color = C_WOOD_LIGHT;
        var ht = UI("HT", tb); Stretch(ht);
        MakeText(ht, "⚠  XÁC NHẬN THOÁT", 32, FontStyle.Bold, C_PAPER, TextAnchor.MiddleCenter, F_TITLE);

        var q = UI("Q", dlg); Anchor(q,0.05f,0.52f,0.95f,0.84f);
        MakeText(q, "Bạn có muốn thoát game?", 20, FontStyle.Normal, C_PAPER, TextAnchor.MiddleCenter);

        var sub = UI("S", dlg); Anchor(sub,0.05f,0.38f,0.95f,0.54f);
        MakeText(sub, "Tiến trình chưa lưu sẽ mất.", 15, FontStyle.Normal,
            C_RICE_GOLD, TextAnchor.MiddleCenter);

        // Nút THOÁT (đỏ)
        MakeDialogBtn(dlg, "✔  THOÁT", "OnClickConfirmExit",
            0.05f,0.06f, 0.46f,0.34f, C_DO_SON);
        // Nút HỦY (xanh tre)
        MakeDialogBtn(dlg, "✖  HỦY", "OnClickCancelExit",
            0.54f,0.06f, 0.95f,0.34f, C_BAMBOO_LT);

        return panel;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  FADE OVERLAY
    // ════════════════════════════════════════════════════════════════════════
    CanvasGroup MakeFadeOverlay(GameObject parent)
    {
        var go = UI("Fade", parent); Stretch(go);
        go.AddComponent<Image>().color = Color.black;
        var cg = go.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable   = false;
        cg.alpha = 1f;
        return cg;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  HELPERS — BUTTONS
    // ════════════════════════════════════════════════════════════════════════
    void MakeMainBtn(GameObject parent, string label, string callback,
                     float yMin, float yMax, Color bgColor)
    {
        var go = UI("Btn_"+label, parent);
        Anchor(go, 0.08f, yMin, 0.92f, yMax);

        // Nền nút bo tròn
        var bg = go.AddComponent<Image>();
        bg.sprite = MakeRoundedSprite(512, 100, 22, bgColor);
        bg.type   = Image.Type.Sliced;
        bg.color  = Color.white;

        // Viền vàng bo tròn (phía sau, lớn hơn 3px)
        var border = UI("Border", go); Stretch(border);
        var brt = border.GetComponent<RectTransform>();
        brt.offsetMin = new Vector2(-3,-3);
        brt.offsetMax = new Vector2(3,3);
        var borderImg = border.AddComponent<Image>();
        borderImg.sprite = MakeRoundedSprite(520, 106, 24, C_WOOD_DARK);
        borderImg.type   = Image.Type.Sliced;
        borderImg.color  = Color.white;
        border.transform.SetAsFirstSibling();

        // Chữ
        var lbl = UI("L", go); Stretch(lbl);
        var txt = lbl.AddComponent<Text>();
        txt.text      = label;
        txt.font      = F_TITLE; // Dùng font chữ thư pháp cổ kính!
        txt.fontSize  = 38;      // Font chữ thư pháp có nét mảnh nên tăng size lên 38 cho rõ đẹp!
        txt.fontStyle = FontStyle.Bold;
        txt.color     = C_PAPER;
        txt.alignment = TextAnchor.MiddleCenter;
        AddOutline(lbl, C_WOOD_DARK, 2.5f);

        // Button
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        var cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1f,0.92f,0.8f,1f);
        cb.pressedColor     = new Color(0.8f,0.65f,0.5f,1f);
        btn.colors = cb;
        string fn = callback;
        btn.onClick.AddListener(() => {
            var managers = Object.FindObjectsByType<MainMenuManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var m = managers.Length > 0 ? managers[0] : null;
            if (m) m.SendMessage(fn);
        });

        // Hover effect
        var vmb = go.AddComponent<VietnamMenuButton>();
        vmb.hoverScale  = 1.05f;
        vmb.normalColor = bgColor;
        vmb.hoverColor  = new Color(0.72f, 0.40f, 0.15f);
        vmb.clickColor  = C_RICE_GOLD;
    }

    void MakeSmallBtn(GameObject parent, string label, string callback, float yMin, float yMax)
    {
        var go = UI("SmBtn_"+label, parent);
        Anchor(go, 0.10f, yMin, 0.90f, yMax);
        var bg = go.AddComponent<Image>();
        bg.sprite = MakeRoundedSprite(400, 80, 18, C_WOOD_LIGHT);
        bg.type   = Image.Type.Sliced;
        bg.color  = Color.white;

        var lbl = UI("L", go); Stretch(lbl);
        var txt = lbl.AddComponent<Text>();
        txt.text = label; txt.font = F_TITLE;
        txt.fontSize = 28; txt.fontStyle = FontStyle.Bold;
        txt.color = C_PAPER; txt.alignment = TextAnchor.MiddleCenter;
        AddOutline(lbl, C_WOOD_DARK, 2f);

        var btn = go.AddComponent<Button>(); btn.targetGraphic = bg;
        var cb2 = btn.colors;
        cb2.normalColor = Color.white; cb2.highlightedColor = new Color(1f,0.9f,0.6f,1f);
        btn.colors = cb2;
        string fn = callback;
        btn.onClick.AddListener(() => {
            var managers = Object.FindObjectsByType<MainMenuManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var m = managers.Length > 0 ? managers[0] : null;
            if (m) m.SendMessage(fn);
        });
        var vmb = go.AddComponent<VietnamMenuButton>();
        vmb.hoverScale = 1.04f; vmb.normalColor = C_WOOD_LIGHT; vmb.hoverColor = new Color(0.72f, 0.40f, 0.15f);
    }

    void MakeDialogBtn(GameObject parent, string label, string callback,
                       float x0, float y0, float x1, float y1, Color bgColor)
    {
        var go = UI("DBtn_"+label, parent);
        Anchor(go, x0, y0, x1, y1);
        go.GetComponent<RectTransform>().offsetMin = new Vector2(6,4);
        go.GetComponent<RectTransform>().offsetMax = new Vector2(-6,-4);
        var bg = go.AddComponent<Image>();
        bg.sprite = MakeRoundedSprite(300, 70, 16, bgColor);
        bg.type   = Image.Type.Sliced;
        bg.color  = Color.white;

        var lbl = UI("L", go); Stretch(lbl);
        var txt = lbl.AddComponent<Text>();
        txt.text = label; txt.font = F_TITLE;
        txt.fontSize = 24; txt.fontStyle = FontStyle.Bold;
        txt.color = C_PAPER; txt.alignment = TextAnchor.MiddleCenter;
        AddOutline(lbl, C_WOOD_DARK, 2f);

        var btn = go.AddComponent<Button>(); btn.targetGraphic = bg;
        var cbl = btn.colors;
        cbl.normalColor = Color.white; cbl.highlightedColor = new Color(1f,0.9f,0.7f,1f);
        btn.colors = cbl;
        string fn = callback;
        btn.onClick.AddListener(() => {
            var managers = Object.FindObjectsByType<MainMenuManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var m = managers.Length > 0 ? managers[0] : null;
            if (m) m.SendMessage(fn);
        });
        var vmb = go.AddComponent<VietnamMenuButton>();
        vmb.hoverScale = 1.04f; vmb.normalColor = bgColor;
        vmb.hoverColor  = bgColor * new Color(1.3f,1.3f,1.3f,1f);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  HELPERS — OPTIONS ROWS
    // ════════════════════════════════════════════════════════════════════════
    void MakeSliderRow(GameObject parent, string label, float yTop)
    {
        // Label
        var lgo = UI("SL_"+label, parent); Anchor(lgo, 0.04f, yTop-0.035f, 0.96f, yTop+0.03f);
        MakeText(lgo, label, 28, FontStyle.Bold, C_PAPER, TextAnchor.MiddleLeft);

        // Slider track
        var sgo  = UI("Slider_"+label, parent); Anchor(sgo, 0.04f, yTop-0.11f, 0.96f, yTop-0.04f);
        var sl   = sgo.AddComponent<Slider>(); sl.minValue=0; sl.maxValue=1; sl.value=0.8f;

        var sbg = UI("Bg",sgo); Stretch(sbg);
        sbg.GetComponent<RectTransform>().offsetMin=new Vector2(0,6);
        sbg.GetComponent<RectTransform>().offsetMax=new Vector2(0,-6);
        sbg.AddComponent<Image>().color = new Color(0.12f, 0.06f, 0.02f, 0.9f);

        var fa = UI("FA",sgo); SetRT(fa, 0,0,1,1, 5,8,-12,-8);
        var fi = UI("F",fa); Stretch(fi);
        var fiImg = fi.AddComponent<Image>(); fiImg.color = C_RICE_GOLD;

        var ha = UI("HA",sgo); SetRT(ha, 0,0,1,1, 8,0,-8,0);
        var h  = UI("H",ha);
        var hrt = h.GetComponent<RectTransform>();
        hrt.anchorMin=new Vector2(0,0); hrt.anchorMax=new Vector2(0,1); hrt.sizeDelta=new Vector2(18,0);
        var hImg = h.AddComponent<Image>(); hImg.color = C_WOOD_LIGHT;

        sl.fillRect = fi.GetComponent<RectTransform>();
        sl.handleRect = h.GetComponent<RectTransform>();
        sl.targetGraphic = hImg;

        // Load saved state
        string key = "vol_master";
        if (label.Contains("Nhạc")) key = "vol_music";
        else if (label.Contains("Hiệu Ứng")) key = "vol_sfx";
        sl.value = PlayerPrefs.GetFloat(key, 0.8f);

        // Listen for changes and update system volume
        sl.onValueChanged.AddListener((val) => {
            PlayerPrefs.SetFloat(key, val);
            PlayerPrefs.Save();
            
            var optUIs = Object.FindObjectsByType<OptionsMenuUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var optUI = optUIs.Length > 0 ? optUIs[0] : null;
            if (optUI != null)
            {
                if (key == "vol_master") optUI.OnMasterVolumeChanged(val);
                else if (key == "vol_music") optUI.OnMusicVolumeChanged(val);
                else if (key == "vol_sfx") optUI.OnSFXVolumeChanged(val);
            }
        });
    }

    void MakeToggleRow(GameObject parent, string label, float yTop)
    {
        var row = UI("TR_"+label, parent); 
        Anchor(row, 0.04f, yTop-0.04f, 0.96f, yTop+0.04f);

        // Label as a child of row
        var lbl = UI("Label", row);
        var lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0.5f);
        lrt.anchorMax = new Vector2(0f, 0.5f);
        lrt.pivot = new Vector2(0f, 0.5f);
        lrt.anchoredPosition = Vector2.zero;

        var txt = lbl.AddComponent<Text>();
        txt.text = label;
        txt.font = F_BODY != null ? F_BODY : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 28;
        txt.fontStyle = FontStyle.Bold;
        txt.color = C_PAPER;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        var fitter = lbl.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Toggle as a child of the label! This guarantees it stays right next to the text.
        var tgo = UI("Tog", lbl);
        var tr = tgo.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(1f, 0.5f); // Anchor to the right edge of the label text
        tr.anchorMax = new Vector2(1f, 0.5f);
        tr.pivot = new Vector2(0f, 0.5f);      // Pivot on the left edge of the toggle
        tr.sizeDelta = new Vector2(36f, 36f);
        tr.anchoredPosition = new Vector2(20f, 0f); // Exactly 20 pixels to the right of the text
        
        var tog = tgo.AddComponent<Toggle>();

        var tbg = UI("Bg", tgo); Stretch(tbg);
        var tbgI = tbg.AddComponent<Image>();
        tbgI.sprite = MakeRoundedSprite(36, 36, 8, new Color(0.12f, 0.06f, 0.02f, 0.9f));
        tbgI.type = Image.Type.Sliced;
        tog.targetGraphic = tbgI;

        var chk = UI("Chk", tbg); Stretch(chk);
        var cr = chk.GetComponent<RectTransform>();
        cr.offsetMin = new Vector2(5, 5);
        cr.offsetMax = new Vector2(-5, -5);
        var chkI = chk.AddComponent<Image>();
        chkI.sprite = MakeRoundedSprite(26, 26, 6, C_RICE_GOLD);
        chkI.type = Image.Type.Sliced;
        tog.graphic = chkI;

        // Load initial state
        tog.isOn = PlayerPrefs.GetInt("fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        // Listener to change Screen.fullScreen and save settings
        tog.onValueChanged.AddListener((isOn) => {
            Screen.fullScreen = isOn;
            PlayerPrefs.SetInt("fullscreen", isOn ? 1 : 0);
            PlayerPrefs.Save();
        });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  HELPERS — DECORATIVE
    // ════════════════════════════════════════════════════════════════════════
    void HLine(GameObject parent, float y)
    {
        var go = UI("Line", parent);
        Anchor(go, 0.03f, y-0.003f, 0.97f, y+0.003f);
        go.AddComponent<Image>().color = new Color(0.88f,0.68f,0.12f,0.60f);
    }

    void BuildBorderFull(GameObject target, Color color, float thickness)
    {
        // Bottom
        var b = UI("BrB",target); var br = b.GetComponent<RectTransform>();
        br.anchorMin=Vector2.zero; br.anchorMax=new Vector2(1,0);
        br.offsetMin=Vector2.zero; br.offsetMax=new Vector2(0,thickness);
        b.AddComponent<Image>().color=color; b.transform.SetAsFirstSibling();
        // Top
        var t2 = UI("BrT",target); var tr2 = t2.GetComponent<RectTransform>();
        tr2.anchorMin=new Vector2(0,1); tr2.anchorMax=Vector2.one;
        tr2.offsetMin=new Vector2(0,-thickness); tr2.offsetMax=Vector2.zero;
        t2.AddComponent<Image>().color=color; t2.transform.SetAsFirstSibling();
        // Left
        var l = UI("BrL",target); var lr = l.GetComponent<RectTransform>();
        lr.anchorMin=Vector2.zero; lr.anchorMax=new Vector2(0,1);
        lr.offsetMin=Vector2.zero; lr.offsetMax=new Vector2(thickness,0);
        l.AddComponent<Image>().color=color; l.transform.SetAsFirstSibling();
        // Right
        var r = UI("BrR",target); var rr = r.GetComponent<RectTransform>();
        rr.anchorMin=new Vector2(1,0); rr.anchorMax=Vector2.one;
        rr.offsetMin=new Vector2(-thickness,0); rr.offsetMax=Vector2.zero;
        r.AddComponent<Image>().color=color; r.transform.SetAsFirstSibling();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  MICRO HELPERS
    // ════════════════════════════════════════════════════════════════════════
    void MakeText(GameObject go, string text, int size, FontStyle style, Color color, TextAnchor align, Font font = null)
    {
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = font != null ? font : (F_BODY != null ? F_BODY : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
        t.fontSize = size; t.fontStyle = style; t.color = color; t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
    }

    void AddOutline(GameObject go, Color color, float dist)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = color;
        o.effectDistance = new Vector2(dist, -dist);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ROUNDED RECT SPRITE
    //  Tạo Sprite hình chữ nhật bo góc bằng code (không cần ảnh ngoài)
    // ════════════════════════════════════════════════════════════════════════
    static Sprite MakeRoundedSprite(int w, int h, int radius, Color col)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[w * h];

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            // Tâm của 4 góc tròn
            int cx = (x < radius) ? radius : (x > w-1-radius ? w-1-radius : x);
            int cy = (y < radius) ? radius : (y > h-1-radius ? h-1-radius : y);
            float dx = x - cx, dy = y - cy;
            float dist2 = dx*dx + dy*dy;
            float r2    = (float)radius * radius;

            if (dist2 <= r2)
                pixels[y * w + x] = col;
            else
                pixels[y * w + x] = Color.clear;
        }

        tex.SetPixels(pixels);
        tex.Apply();

        // Border = radius ở 4 cạnh để Image.Type.Sliced scale đúng
        var border = new Vector4(radius, radius, radius, radius);
        return Sprite.Create(tex,
            new Rect(0,0,w,h), new Vector2(0.5f,0.5f), 100f,
            0, SpriteMeshType.FullRect, border);
    }
    void BuildBraidedRope(GameObject ropeGo)
    {
        for (int y = 0; y < 240; y += 6)
        {
            var segment = UI("Seg_" + y, ropeGo);
            var rt = segment.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(16f, 10f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.localRotation = Quaternion.Euler(0f, 0f, 15f * (y % 12 == 0 ? 1 : -1));
            
            var img = segment.AddComponent<Image>();
            img.sprite = MakeRoundedSprite(32, 20, 8, C_ROPE);
            img.type = Image.Type.Sliced;
            
            var border = UI("B", segment); Stretch(border);
            border.AddComponent<Image>().sprite = MakeRoundedSprite(34, 22, 9, C_WOOD_DARK);
            border.transform.SetAsFirstSibling();
        }
    }

    void BuildTiedKnot(GameObject knotGo)
    {
        var inner = UI("Hole", knotGo);
        var rt = inner.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = Vector2.one * 0.5f;
        rt.sizeDelta = new Vector2(14f, 14f);
        var img = inner.AddComponent<Image>();
        img.sprite = MakeRoundedSprite(30, 30, 15, C_WOOD_DARK);
    }


    static GameObject UI(string name, GameObject parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Anchor(GameObject go, float x0, float y0, float x1, float y1)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0,y0); rt.anchorMax = new Vector2(x1,y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void SetRT(GameObject go, float ax0,float ay0,float ax1,float ay1,
                      float ox0,float oy0,float ox1,float oy1)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax0,ay0); rt.anchorMax = new Vector2(ax1,ay1);
        rt.offsetMin = new Vector2(ox0,oy0); rt.offsetMax = new Vector2(ox1,oy1);
    }
}

public class FloatingParticle : MonoBehaviour
{
    public float speed;
    public float rotSpeed;
    public float windFrequency;
    public float windAmplitude;
    public bool isLeaf;
    
    private RectTransform rt;
    private float startY;
    private float timeOffset;
    
    void Start()
    {
        rt = GetComponent<RectTransform>();
        startY = rt.anchoredPosition.y;
        timeOffset = Random.Range(0f, 100f);
    }
    
    void Update()
    {
        Vector2 pos = rt.anchoredPosition;
        pos.x -= speed * Time.deltaTime;
        pos.y = startY + Mathf.Sin(Time.time * windFrequency + timeOffset) * windAmplitude;
        
        rt.anchoredPosition = pos;
        rt.Rotate(0f, 0f, rotSpeed * Time.deltaTime);
        
        if (pos.x < -100f)
        {
            pos.x = 2020f;
            startY = Random.Range(50f, 1030f);
            pos.y = startY;
            rt.anchoredPosition = pos;
        }
    }
}

public class SlowHover : MonoBehaviour
{
    public float hoverSpeed = 1.5f;
    public float hoverRange = 10f;
    
    private RectTransform rt;
    private Vector2 startPos;
    
    void Start()
    {
        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition;
    }
    
    void Update()
    {
        Vector2 pos = startPos;
        pos.y += Mathf.Sin(Time.time * hoverSpeed) * hoverRange;
        rt.anchoredPosition = pos;
    }
}
