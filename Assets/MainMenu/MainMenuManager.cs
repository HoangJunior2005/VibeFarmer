using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý màn hình chính (Main Menu) của game Thôn An Lúa.
/// Gắn script này vào một GameObject tên "MainMenuManager" trong Scene MainMenu.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("=== CÁC PANEL UI ===")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject confirmExitPanel;

    [Header("=== TÊN SCENE ===")]
    [Tooltip("Tên Scene gameplay chính cần load")]
    public string gameSceneName = "TownScene";

    [Header("=== HIỆU ỨNG ===")]
    public float fadeDuration = 0.8f;
    public CanvasGroup fadeOverlay; // Một Image đen phủ toàn màn hình

    // ── Trạng thái nội bộ ──────────────────────────────────────────────────
    private bool _isTransitioning = false;

    // ───────────────────────────────────────────────────────────────────────
    void Awake()
    {
        // Đảm bảo chỉ hiện Main Menu khi khởi động
        ShowPanel(mainMenuPanel);
    }

    void Start()
    {
        // Fade in từ đen lên
        if (fadeOverlay != null)
            StartCoroutine(FadeFromBlack());
    }

    // ── CÁC NÚT MAIN MENU ─────────────────────────────────────────────────

    /// <summary>Nút "Bắt Đầu" — Load scene gameplay</summary>
    public void OnClickStart()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        StartCoroutine(LoadGameScene());
    }

    /// <summary>Nút "Tùy Chọn" — Mở panel Options</summary>
    public void OnClickOptions()
    {
        if (_isTransitioning) return;
        ShowPanel(optionsPanel);
    }

    /// <summary>Nút "Thoát" — Hiện hộp xác nhận thoát</summary>
    public void OnClickExit()
    {
        if (_isTransitioning) return;
        ShowPanel(confirmExitPanel);
    }

    // ── CÁC NÚT OPTIONS PANEL ─────────────────────────────────────────────

    /// <summary>Nút "Quay Lại" từ Options</summary>
    public void OnClickBackFromOptions()
    {
        ShowPanel(mainMenuPanel);
    }

    // ── CÁC NÚT CONFIRM EXIT ──────────────────────────────────────────────

    /// <summary>Xác nhận thoát game</summary>
    public void OnClickConfirmExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>Hủy thoát, quay lại Main Menu</summary>
    public void OnClickCancelExit()
    {
        ShowPanel(mainMenuPanel);
    }

    // ── HELPER ────────────────────────────────────────────────────────────

    private void ShowPanel(GameObject target)
    {
        if (mainMenuPanel   != null) mainMenuPanel.SetActive(false);
        if (optionsPanel    != null) optionsPanel.SetActive(false);
        if (confirmExitPanel!= null) confirmExitPanel.SetActive(false);

        if (target != null) target.SetActive(true);
    }

    private IEnumerator LoadGameScene()
    {
        // Fade to black
        if (fadeOverlay != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
        }

        var sceneToLoad = string.IsNullOrEmpty(gameSceneName) ? "TownScene" : gameSceneName;
        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeOverlay == null) yield break;
        fadeOverlay.alpha = 1f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeOverlay.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = 0f;
    }
}
