using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Hiệu ứng hover/click cho các nút menu Việt Nam.
/// Gắn vào từng Button trong Main Menu.
/// </summary>
[RequireComponent(typeof(Button))]
public class VietnamMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("=== HIỆU ỨNG NÚT ===")]
    public float hoverScale    = 1.08f;
    public float normalScale   = 1.0f;
    public float animSpeed     = 8f;

    [Header("=== MÀU SẮC ===")]
    public Color normalColor   = new Color(0.60f, 0.15f, 0.05f, 0.92f); // Đỏ son
    public Color hoverColor    = new Color(0.85f, 0.62f, 0.05f, 0.97f); // Vàng đồng
    public Color clickColor    = new Color(1.00f, 0.90f, 0.30f, 1.00f); // Vàng sáng

    [Header("=== ICON TRANG TRÍ (tùy chọn) ===")]
    public GameObject leftDecor;   // Biểu tượng hoa sen bên trái
    public GameObject rightDecor;  // Biểu tượng hoa sen bên phải

    // Private
    private RectTransform _rect;
    private Image         _bg;
    private Vector3       _targetScale;
    private Color         _targetColor;
    private bool          _hovered;

    void Awake()
    {
        _rect  = GetComponent<RectTransform>();
        _bg    = GetComponent<Image>();

        _targetScale = Vector3.one * normalScale;
        _targetColor = normalColor;
        if (_bg != null) _bg.color = normalColor;
    }

    void Update()
    {
        // Scale animation
        _rect.localScale = Vector3.Lerp(_rect.localScale, _targetScale, Time.deltaTime * animSpeed);

        // Color animation
        if (_bg != null)
            _bg.color = Color.Lerp(_bg.color, _targetColor, Time.deltaTime * animSpeed);

        // Decor animation
        if (leftDecor  != null) leftDecor.SetActive(_hovered);
        if (rightDecor != null) rightDecor.SetActive(_hovered);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        _hovered     = true;
        _targetScale = Vector3.one * hoverScale;
        _targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData e)
    {
        _hovered     = false;
        _targetScale = Vector3.one * normalScale;
        _targetColor = normalColor;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (gameObject.activeInHierarchy)
        {
            _targetColor = clickColor;
            StartCoroutine(ResetColorAfterDelay(0.15f));
        }
        else
        {
            _targetColor = normalColor;
            if (_bg != null) _bg.color = normalColor;
        }
    }

    private IEnumerator ResetColorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _targetColor = _hovered ? hoverColor : normalColor;
    }

    void OnDisable()
    {
        _hovered = false;
        _targetScale = Vector3.one * normalScale;
        _targetColor = normalColor;
        if (_rect != null) _rect.localScale = Vector3.one * normalScale;
        if (_bg != null) _bg.color = normalColor;
    }
}
