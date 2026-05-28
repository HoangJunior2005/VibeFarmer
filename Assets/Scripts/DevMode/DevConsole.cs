using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Developer Mode Console
/// Tự động tạo trong mọi scene — KHÔNG cần kéo thả vào scene.
/// Nhấn F1 để mở/đóng. Gõ "help" để xem lệnh.
/// </summary>
public class DevConsole : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────
    public static DevConsole Instance { get; private set; }

    // ─── Auto Bootstrap (tự động tạo khi game chạy) ───────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[DevConsole]");
        DontDestroyOnLoad(go);
        go.AddComponent<DevConsole>();
    }

    // ─── Config ───────────────────────────────────────────────────
    private KeyCode toggleKey = KeyCode.F1;

    // ─── References ───────────────────────────────────────────────
    private TimeManager timeManager;

    // ─── State ────────────────────────────────────────────────────
    private bool isOpen     = false;
    private bool showFPS    = false;
    private bool godMode    = false;

    private string inputText    = "";
    private string pendingSubmit = null; // bridge Update → OnGUI
    private readonly List<string> log = new List<string>();
    private Vector2 scroll;

    // Player speed
    private float originalAgentSpeed = -1f;

    // FPS
    private float fpsTimer;
    private int   fpsDisplay;

    // GUI styles (built lazily inside OnGUI)
    private GUIStyle styleHeader;
    private GUIStyle styleLog;
    private GUIStyle styleInput;
    private bool     stylesBuilt = false;

    // Layout
    private const float W  = 680f;
    private const float H  = 370f;
    private const float PAD = 8f;
    private const float HDR = 30f;
    private const float INP = 30f;

    // ─────────────────────────────────────────────────────────────
    #region Unity
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
        AddLog("<color=#44FF88>[DevConsole] Sẵn sàng — nhấn <b>F1</b> để mở, gõ <b>help</b> để xem lệnh.</color>");
    }

    private void Update()
    {
        // Toggle console
        if (Input.GetKeyDown(toggleKey))
        {
            isOpen = !isOpen;
            if (isOpen) inputText = "";
        }

        // Bắt Enter trong Update để tránh game ăn mất phím
        if (isOpen && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            string cmd = inputText.Trim();
            inputText = "";
            scroll    = new Vector2(0, float.MaxValue);
            if (!string.IsNullOrEmpty(cmd))
                pendingSubmit = cmd;
        }

        // Xử lý lệnh pending (phải gọi ngoài OnGUI)
        if (pendingSubmit != null)
        {
            RunCommand(pendingSubmit);
            pendingSubmit = null;
        }

        // FPS tick
        fpsTimer += Time.unscaledDeltaTime;
        if (fpsTimer >= 0.5f)
        {
            fpsDisplay = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
            fpsTimer   = 0f;
        }
    }

    private void OnGUI()
    {
        BuildStyles();

        // FPS góc trên phải
        if (showFPS)
        {
            Color fc = fpsDisplay >= 60 ? Color.green : fpsDisplay >= 30 ? Color.yellow : Color.red;
            var s = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
            s.normal.textColor = fc;
            GUI.Label(new Rect(Screen.width - 90, 6, 85, 22), $"FPS {fpsDisplay}", s);
        }

        if (!isOpen) return;

        // Dim overlay
        var dim = new Texture2D(1, 1);
        dim.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
        dim.Apply();
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dim);

        // Window position (giữa màn hình)
        float x = (Screen.width  - W) * 0.5f;
        float y = (Screen.height - H) * 0.5f;

        // Border glow
        Rect border = new Rect(x - 2, y - 2, W + 4, H + 4);
        DrawSolidRect(border, new Color(0.2f, 0.9f, 0.45f));

        // Background
        DrawSolidRect(new Rect(x, y, W, H), new Color(0.04f, 0.06f, 0.09f, 0.98f));

        // Header bar
        DrawSolidRect(new Rect(x, y, W, HDR), new Color(0.08f, 0.55f, 0.28f));
        string headerText = godMode
            ? "⚙  DEV CONSOLE  |  F1 đóng          ✦ GODMODE ON"
            : "⚙  DEV CONSOLE  |  F1 đóng";
        GUI.Label(new Rect(x + PAD, y + 5, W - PAD * 2, HDR), headerText, styleHeader);

        // Log scroll area
        float logH = H - HDR - INP - PAD * 3;
        Rect logBox = new Rect(x + PAD, y + HDR + PAD, W - PAD * 2, logH);
        DrawSolidRect(logBox, new Color(0f, 0f, 0f, 0.55f));

        GUILayout.BeginArea(logBox);
        scroll = GUILayout.BeginScrollView(scroll, false, false,
                     GUIStyle.none, GUIStyle.none);
        foreach (var line in log)
            GUILayout.Label(line, styleLog);
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        // Input bar
        float iy = y + H - INP - PAD;
        DrawSolidRect(new Rect(x + PAD, iy, W - PAD * 2, INP), new Color(0.08f, 0.12f, 0.08f));

        GUI.SetNextControlName("devinput");
        inputText = GUI.TextField(
            new Rect(x + PAD + 4, iy + 4, W - PAD * 2 - 8, INP - 8),
            inputText, styleInput);
        GUI.FocusControl("devinput");

        // Consume Enter trong OnGUI để tránh nhân đôi lệnh
        Event ev = Event.current;
        if (ev.type == EventType.KeyDown &&
            (ev.keyCode == KeyCode.Return || ev.keyCode == KeyCode.KeypadEnter))
        {
            ev.Use();
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Commands
    // ─────────────────────────────────────────────────────────────

    private void RunCommand(string raw)
    {
        AddLog($"<color=#AAFFAA>> {raw}</color>");
        string[] p = raw.Split(' ');
        string cmd = p[0].ToLower();

        switch (cmd)
        {
            case "help":      ShowHelp();                          break;
            case "clear":     log.Clear();                         break;
            case "fps":       showFPS = !showFPS;
                              AddLog($"FPS counter: <b>{(showFPS?"BẬT":"TẮT")}</b>"); break;
            case "godmode":   godMode = !godMode;
                              AddLog($"God Mode: <b><color={(godMode?"#FFD700":"#FF6666")}>{(godMode?"BẬT":"TẮT")}</color></b>"); break;
            case "money":     CmdMoney(p, add:true);               break;
            case "setmoney":  CmdMoney(p, add:false);              break;
            case "additem":   CmdAddItem(p);                       break;
            case "sethour":   CmdSetHour(p);                      break;
            case "timescale": CmdTimeScale(p);                     break;
            case "speed":     CmdSpeed(p);                         break;
            default:
                AddLog($"<color=#FF5555>Lệnh không hợp lệ: '{cmd}'. Gõ <b>help</b>.</color>");
                break;
        }
    }

    private void ShowHelp()
    {
        AddLog("──────────────────────────────────────────");
        AddLog("<color=#44FF88><b>LỆNH DEV CONSOLE</b></color>");
        AddLog("  <b>money</b> [số]          — thêm tiền vào ví");
        AddLog("  <b>setmoney</b> [số]       — đặt tiền về đúng số");
        AddLog("  <b>additem</b> [code] [qty]— thêm item vào túi");
        AddLog("  <b>sethour</b> [0-23]      — đặt giờ game");
        AddLog("  <b>timescale</b> [x]       — tốc độ thời gian game");
        AddLog("  <b>speed</b> [x]           — tốc độ nhân vật (NavMeshAgent)");
        AddLog("  <b>fps</b>                 — bật/tắt FPS counter");
        AddLog("  <b>godmode</b>             — bật/tắt God Mode flag");
        AddLog("  <b>clear</b>               — xóa log");
        AddLog("──────────────────────────────────────────");
    }

    private void CmdMoney(string[] p, bool add)
    {
        if (p.Length < 2 || !int.TryParse(p[1], out int amount))
        { AddLog("<color=#FF5555>Cú pháp: money [số]</color>"); return; }

        if (add)
        {
            InventoryManager.Instance.AddMoney(amount);
        }
        else
        {
            int cur = InventoryManager.Instance.GetMoneyInInventory();
            InventoryManager.Instance.RemoveMoney(cur);
            InventoryManager.Instance.AddMoney(amount);
        }
        AddLog($"<color=#FFD700>Tiền hiện tại: <b>{InventoryManager.Instance.GetMoneyInInventory():N0} ₫</b></color>");
    }

    private void CmdAddItem(string[] p)
    {
        if (p.Length < 2 || !int.TryParse(p[1], out int code))
        { AddLog("<color=#FF5555>Cú pháp: additem [itemCode] [qty]</color>"); return; }

        int qty = 1;
        if (p.Length >= 3) int.TryParse(p[2], out qty);
        qty = Mathf.Max(1, qty);

        ItemDetails det = InventoryManager.Instance.GetItemDetails(code);
        if (det == null) { AddLog($"<color=#FF5555>Không tìm thấy item code {code}</color>"); return; }

        for (int i = 0; i < qty; i++)
            InventoryManager.Instance.AddItem(InventoryLocation.player, code);

        AddLog($"<color=#44FF88>+{qty}x <b>{det.itemDescription}</b> (code {code})</color>");
    }

    private void CmdSetHour(string[] p)
    {
        if (timeManager == null) { AddLog("<color=#FF5555>Không tìm thấy TimeManager</color>"); return; }
        if (p.Length < 2 || !int.TryParse(p[1], out int h) || h < 0 || h > 23)
        { AddLog("<color=#FF5555>Cú pháp: sethour [0-23]</color>"); return; }
        timeManager.setHour(h);
        AddLog($"<color=#87CEEB>Giờ → <b>{h:D2}:00</b></color>");
    }

    private void CmdTimeScale(string[] p)
    {
        if (timeManager == null) { AddLog("<color=#FF5555>Không tìm thấy TimeManager</color>"); return; }
        if (p.Length < 2 || !float.TryParse(p[1], System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float ts) || ts <= 0)
        { AddLog("<color=#FF5555>Cú pháp: timescale [số > 0]</color>"); return; }
        timeManager.SpeedUpTime(ts);
        AddLog($"<color=#87CEEB>Tốc độ game time: <b>x{ts}</b></color>");
    }

    private void CmdSpeed(string[] p)
    {
        if (p.Length < 2 || !float.TryParse(p[1], System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float spd) || spd <= 0)
        { AddLog("<color=#FF5555>Cú pháp: speed [số > 0]</color>"); return; }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) { AddLog("<color=#FF5555>Không tìm thấy Player (tag 'Player')</color>"); return; }

        var agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            if (originalAgentSpeed < 0f) originalAgentSpeed = agent.speed;
            agent.speed = originalAgentSpeed * spd;
            AddLog($"<color=#44FF88>Player speed: <b>{agent.speed:F2}</b> (x{spd})</color>");
        }
        else
        {
            AddLog("<color=#FFA500>Không tìm thấy NavMeshAgent trên Player.</color>");
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────
    #region Helpers
    // ─────────────────────────────────────────────────────────────

    public void AddLog(string msg)
    {
        log.Add(msg);
        if (log.Count > 300) log.RemoveAt(0);
        scroll = new Vector2(0, float.MaxValue);
    }

    private void DrawSolidRect(Rect r, Color c)
    {
        Color old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }

    private void BuildStyles()
    {
        if (stylesBuilt) return;

        styleHeader = new GUIStyle(GUI.skin.label)
        {
            fontSize   = 13,
            fontStyle  = FontStyle.Bold,
            richText   = true,
        };
        styleHeader.normal.textColor = Color.white;

        styleLog = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 12,
            richText  = true,
            wordWrap  = true,
        };
        styleLog.normal.textColor = new Color(0.88f, 0.97f, 0.88f);

        styleInput = new GUIStyle(GUI.skin.textField)
        {
            fontSize = 13,
        };
        styleInput.normal.textColor  = new Color(0.8f, 1f, 0.8f);
        styleInput.focused.textColor = Color.white;

        stylesBuilt = true;
    }

    /// <summary>Dùng để kiểm tra God Mode từ script khác</summary>
    public bool IsGodMode() => godMode;

    #endregion
}
