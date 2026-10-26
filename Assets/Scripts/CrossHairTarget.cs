using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CrossHairTarget : MonoBehaviour
{
    [SerializeField] private Image _crosshairImage;
    [SerializeField] private float _heightOffset = 1.0f; // Высота смещения (центр тела/грудь)
    [SerializeField] private Vector3 _screenOffset = Vector2.zero; // Дополнительное экранное смещение при необходимости

    private Transform _currentTarget;
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Camera _cam;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();

        if (_canvas != null)
        {
            _cam = _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
        }

        if (_crosshairImage == null)
        {
            _crosshairImage = GetComponent<Image>();
        }

        // По умолчанию скрываем прицел
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_currentTarget == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Точка в мировом пространстве (с учетом роста врага)
        Vector3 worldPos = _currentTarget.position + Vector3.up * _heightOffset;

        // Если камера сзади объекта — не отображаем прицел
        if (_cam != null)
        {
            Vector3 viewPos = _cam.WorldToViewportPoint(worldPos);
            if (viewPos.z <= 0)
            {
                if (_crosshairImage != null) _crosshairImage.enabled = false;
                return;
            }
            if (_crosshairImage != null) _crosshairImage.enabled = true;
        }

        // Преобразование мировых координат в локальные координаты Canvas
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_cam, worldPos);
        RectTransform canvasRect = _canvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, _cam, out Vector2 localPoint))
        {
            _rectTransform.anchoredPosition = localPoint + (Vector2)_screenOffset;
        }
    }

    public void SetTarget(Transform target)
    {
        _currentTarget = target;
        gameObject.SetActive(true);
        if (_crosshairImage != null) _crosshairImage.enabled = true;
    }

    public void ClearTarget()
    {
        _currentTarget = null;
        gameObject.SetActive(false);
    }
}