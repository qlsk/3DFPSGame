using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 버튼에 마우스를 올리면 커지고, 누르면 작아졌다가 돌아오는 반응을 준다.
[RequireComponent(typeof(Button))]
public class UI_ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("크기")]
    [SerializeField] private float _hoverScale = 1.05f;
    [SerializeField] private float _pressScale = 0.95f;

    [Header("속도")]
    [SerializeField] private float _scaleSpeed = 15f;

    private Button _button;
    private Vector3 _baseScale;
    private bool _isHovered;
    private bool _isPressed;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _baseScale = transform.localScale;
    }

    private void OnDisable()
    {
        // 그룹을 껐다 켜도 커진 상태로 남지 않게 초기화한다.
        _isHovered = false;
        _isPressed = false;
        transform.localScale = _baseScale;
    }

    private void Update()
    {
        // 1. 상태에 맞는 목표 배율을 정한다.
        float targetScale = 1f;
        if (_button.interactable)
        {
            if (_isPressed)
            {
                targetScale = _pressScale;
            }
            else if (_isHovered)
            {
                targetScale = _hoverScale;
            }
        }

        // 2. 목표 크기로 부드럽게 다가간다. 일시정지(timeScale 0)에서도 동작하도록 unscaled 시간을 쓴다.
        float t = 1f - Mathf.Exp(-_scaleSpeed * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, _baseScale * targetScale, t);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        _isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
    }
}
