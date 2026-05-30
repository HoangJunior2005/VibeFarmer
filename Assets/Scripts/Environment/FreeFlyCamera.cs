using UnityEngine;

/// <summary>
/// FreeFlyCamera — Tự tạo một camera bay độc lập trong scene.
///
/// CÁCH GẮN (chỉ cần làm 1 lần):
///   1. Hierarchy → chuột phải → Create Empty → đặt tên "FreeFlyManager"
///   2. Kéo script FreeFlyCamera vào GameObject "FreeFlyManager"
///   3. Bấm Play
///
/// PHÍM [F]: Bật/Tắt chế độ bay
///   • Chuột Phải (giữ)  → Nhìn xung quanh
///   • W / A / S / D     → Bay ngang
///   • E                 → Bay lên
///   • Q                 → Bay xuống
///   • Shift             → Nhanh x3
///   • Scroll wheel      → Zoom
///
/// Khi TẮT: Camera bay bị vô hiệu, Main Camera hoạt động bình thường trở lại.
/// </summary>
[AddComponentMenu("Thôn An Lúa/Camera/Free Fly Camera")]
public class FreeFlyCamera : MonoBehaviour
{
    [Header("Phím bật/tắt")]
    public KeyCode toggleKey = KeyCode.F;

    [Header("Tốc độ di chuyển")]
    [Range(1f, 80f)]  public float moveSpeed       = 15f;
    [Range(1f, 10f)]  public float shiftMultiplier = 3f;

    [Header("Tốc độ nhìn (chuột phải)")]
    [Range(0.5f, 10f)] public float lookSensitivity = 3f;

    [Header("Vị trí khởi đầu khi bật fly mode")]
    [Tooltip("Độ cao camera khi mới bật (đơn vị Unity)")]
    [Range(2f, 50f)] public float startHeight = 15f;
    [Tooltip("Góc nhìn xuống khi mới bật (độ)")]
    [Range(10f, 80f)] public float startPitch  = 40f;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private bool   _flyMode = false;
    private Camera _flyCamera;
    private Camera _mainCamera;

    private float  _pitch, _yaw;
    private float  _hintTimer;
    private const  float HintDuration = 4f;

    private GUIStyle _boxStyle;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        _mainCamera = Camera.main;
        CreateFlyCamera();
    }

    /// <summary>Tạo camera bay mới, gắn vào scene độc lập.</summary>
    private void CreateFlyCamera()
    {
        // Tạo GameObject mới hoàn toàn độc lập
        var go = new GameObject("[FlyCamera]");
        DontDestroyOnLoad(go);   // tồn tại qua scene load

        _flyCamera = go.AddComponent<Camera>();

        // Copy cài đặt cơ bản từ Main Camera
        if (_mainCamera != null)
        {
            _flyCamera.fieldOfView  = _mainCamera.fieldOfView;
            _flyCamera.nearClipPlane= _mainCamera.nearClipPlane;
            _flyCamera.farClipPlane = _mainCamera.farClipPlane;
            _flyCamera.cullingMask  = _mainCamera.cullingMask;
        }

        // Đặt độ ưu tiên thấp hơn main camera (depth thấp hơn)
        // — nhưng khi fly mode ON sẽ nâng depth lên trên
        _flyCamera.depth  = -5;
        _flyCamera.enabled= false;   // tắt ngay, chỉ bật khi cần

        // Vị trí ban đầu: trên không, nhìn xuống
        Vector3 center = _mainCamera != null
            ? _mainCamera.transform.position
            : Vector3.zero;
        go.transform.position = center + new Vector3(0, startHeight, -startHeight * 0.5f);
        go.transform.rotation = Quaternion.Euler(startPitch, 0f, 0f);

        _yaw   = 0f;
        _pitch = startPitch;
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleFlyMode();

        if (!_flyMode || _flyCamera == null) return;

        HandleLook();
        HandleMove();
        HandleZoom();
    }

    // ── Bật / Tắt ─────────────────────────────────────────────────────────────
    private void ToggleFlyMode()
    {
        _flyMode = !_flyMode;

        if (_flyMode)
        {
            // Đặt fly camera lên vị trí phía trên main camera rồi bật
            if (_mainCamera != null)
            {
                Vector3 mc = _mainCamera.transform.position;
                _flyCamera.transform.position = mc + new Vector3(0, startHeight, -startHeight * 0.4f);
                _flyCamera.transform.rotation = Quaternion.Euler(startPitch, 0f, 0f);
                _pitch = startPitch;
                _yaw   = 0f;
            }

            // Bật camera bay, tăng depth để render đè lên main
            _flyCamera.enabled = true;
            _flyCamera.depth   = (_mainCamera != null ? _mainCamera.depth : 0) + 10;

            // Tắt audio listener trên fly camera (tránh warning 2 listeners)
            var al = _flyCamera.GetComponent<AudioListener>();
            if (al != null) al.enabled = false;
        }
        else
        {
            // Tắt camera bay — main camera hiện ra tự động
            _flyCamera.enabled   = false;
            _flyCamera.depth     = -5;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        _hintTimer = HintDuration;
    }

    // ── Nhìn (giữ chuột phải) ─────────────────────────────────────────────────
    private void HandleLook()
    {
        if (!Input.GetMouseButton(1))
        {
            Cursor.visible   = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Locked;

        _yaw   += Input.GetAxis("Mouse X") * lookSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
        _pitch  = Mathf.Clamp(_pitch, -89f, 89f);

        _flyCamera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    // ── Di chuyển ─────────────────────────────────────────────────────────────
    private void HandleMove()
    {
        float speed = moveSpeed * Time.deltaTime
                      * (Input.GetKey(KeyCode.LeftShift) ? shiftMultiplier : 1f);

        Transform t = _flyCamera.transform;
        if (Input.GetKey(KeyCode.W)) t.position += t.forward * speed;
        if (Input.GetKey(KeyCode.S)) t.position -= t.forward * speed;
        if (Input.GetKey(KeyCode.D)) t.position += t.right   * speed;
        if (Input.GetKey(KeyCode.A)) t.position -= t.right   * speed;
        if (Input.GetKey(KeyCode.E)) t.position += Vector3.up * speed;
        if (Input.GetKey(KeyCode.Q)) t.position -= Vector3.up * speed;
    }

    // ── Zoom ──────────────────────────────────────────────────────────────────
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;
        _flyCamera.fieldOfView = Mathf.Clamp(
            _flyCamera.fieldOfView - scroll * 80f, 15f, 100f);
    }

    // ── GUI ───────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        if (_boxStyle == null)
        {
            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize  = 13,
                alignment = TextAnchor.MiddleCenter
            };
            _boxStyle.normal.textColor = Color.white;
        }

        string badge = _flyMode ? "✈ FLY MODE  [F] Tắt" : "[F] Bật chế độ bay";
        GUI.backgroundColor = _flyMode
            ? new Color(0.05f, 0.5f, 0.1f, 0.9f)
            : new Color(0f, 0f, 0f, 0.55f);
        GUI.Box(new Rect(Screen.width - 205, 10, 195, 30), badge, _boxStyle);

        if (_flyMode && _hintTimer > 0f)
        {
            _hintTimer -= Time.deltaTime;
            GUI.backgroundColor = new Color(0, 0, 0, 0.82f);
            GUI.Box(new Rect(Screen.width - 240, 48, 230, 145),
                "✈ Chế Độ Bay\n" +
                "Chuột Phải  → Nhìn\n" +
                "W A S D     → Di chuyển\n" +
                "E / Q       → Lên / Xuống\n" +
                "Shift       → Nhanh x3\n" +
                "Scroll      → Zoom\n" +
                "[F]         → Tắt & khôi phục", _boxStyle);
        }

        GUI.backgroundColor = Color.white;
    }

    private void OnDestroy()
    {
        if (_flyCamera != null)
            Destroy(_flyCamera.gameObject);
    }
}
