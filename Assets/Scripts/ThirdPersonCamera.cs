using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Link")]
    [SerializeField] private Transform _target; // Объект, за которым следит камера (наш игрок)

    [Header("Параметры орбиты")]
    [SerializeField] private float _distance = 5.0f;       // Дистанция от игрока
    [SerializeField] private float _sensitivityX = 0.15f;  // Чувствительность мыши по X
    [SerializeField] private float _sensitivityY = 0.1f;   // Чувствительность мыши по Y
    [SerializeField] private float _minPitch = -20f;       // Ограничение наклона вниз
    [SerializeField] private float _maxPitch = 70f;        // Ограничение наклона вверх

    private InputSystem_Actions _inputActions;
    private float _yaw;   // Угол поворота по горизонтали
    private float _pitch; // Угол наклона по вертикали

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        
        // Инициализируем стартовые углы камеры на основе её текущего поворота на сцене
        Vector3 currentEuler = transform.eulerAngles;
        _yaw = currentEuler.y;
        _pitch = currentEuler.x;
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void Start()
    {
        // Скрываем и блокируем курсор мыши при старте игры
        LockCursor();
    }

    private void Update()
    {
        // Даем игроку возможность разблокировать мышь по нажатию Escape (например для меню)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                UnlockCursor();
            else
                LockCursor();
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // Считываем Mouse Delta (изменение позиции мыши за кадр) из Input Action Asset
            Vector2 lookInput = _inputActions.Player.Look.ReadValue<Vector2>();

            // Накапливаем углы вращения
            _yaw += lookInput.x * _sensitivityX;
            _pitch -= lookInput.y * _sensitivityY; // Инвертируем Y, чтобы движение мыши вверх поднимало камеру

            // Ограничиваем вертикальный угол, чтобы не проваливаться под землю
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        }

        // Вычисляем новое вращение камеры
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);

        // Находим целевую позицию камеры (смещаем ее назад от игрока на расстояние _distance)
        Vector3 position = _target.position + rotation * (Vector3.back * _distance);

        // Применяем позицию и поворот
        transform.rotation = rotation;
        transform.position = position;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}