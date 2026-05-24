using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý Panel Tùy Chọn (Options) — Âm lượng & Chất lượng đồ họa.
/// Gắn vào GameObject "OptionsPanel".
/// </summary>
public class OptionsMenuUI : MonoBehaviour
{
    [Header("=== AUDIO ===")]
    public AudioMixer audioMixer;          // Gán AudioMixer nếu có
    public Slider masterVolumeSlider;      // Slider âm lượng tổng
    public Slider musicVolumeSlider;       // Slider âm nhạc
    public Slider sfxVolumeSlider;         // Slider hiệu ứng âm thanh

    [Header("=== ĐỒ HỌA ===")]
    public TMP_Dropdown qualityDropdown;   // Dropdown chất lượng đồ họa
    public Toggle fullscreenToggle;        // Toggle toàn màn hình

    // PlayerPrefs keys
    private const string KEY_MASTER = "vol_master";
    private const string KEY_MUSIC  = "vol_music";
    private const string KEY_SFX    = "vol_sfx";
    private const string KEY_QUALITY= "quality";
    private const string KEY_FULL   = "fullscreen";

    // ───────────────────────────────────────────────────────────────────────
    void OnEnable()
    {
        LoadSettings();
    }

    // ── AUDIO ──────────────────────────────────────────────────────────────

    public void OnMasterVolumeChanged(float value)
    {
        SetMixerVolume("MasterVolume", value);
        PlayerPrefs.SetFloat(KEY_MASTER, value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        SetMixerVolume("MusicVolume", value);
        PlayerPrefs.SetFloat(KEY_MUSIC, value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        SetMixerVolume("SFXVolume", value);
        PlayerPrefs.SetFloat(KEY_SFX, value);
    }

    // ── ĐỒ HỌA ────────────────────────────────────────────────────────────

    public void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt(KEY_QUALITY, index);
    }

    public void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(KEY_FULL, isFullscreen ? 1 : 0);
    }

    // ── LƯU / TẢI ─────────────────────────────────────────────────────────

    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        // Âm lượng
        float master = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
        float music  = PlayerPrefs.GetFloat(KEY_MUSIC,  1f);
        float sfx    = PlayerPrefs.GetFloat(KEY_SFX,    1f);

        if (masterVolumeSlider != null) { masterVolumeSlider.value = master; SetMixerVolume("MasterVolume", master); }
        if (musicVolumeSlider  != null) { musicVolumeSlider.value  = music;  SetMixerVolume("MusicVolume",  music);  }
        if (sfxVolumeSlider    != null) { sfxVolumeSlider.value    = sfx;    SetMixerVolume("SFXVolume",    sfx);    }

        // Đồ họa
        int quality    = PlayerPrefs.GetInt(KEY_QUALITY, QualitySettings.GetQualityLevel());
        bool fullscreen= PlayerPrefs.GetInt(KEY_FULL, Screen.fullScreen ? 1 : 0) == 1;

        if (qualityDropdown   != null) qualityDropdown.value    = quality;
        if (fullscreenToggle  != null) fullscreenToggle.isOn    = fullscreen;
    }

    private void SetMixerVolume(string paramName, float sliderValue)
    {
        if (audioMixer == null) return;
        // Chuyển đổi slider (0..1) sang dB (-80..0)
        float db = sliderValue > 0.001f ? Mathf.Log10(sliderValue) * 20f : -80f;
        audioMixer.SetFloat(paramName, db);
    }
}
