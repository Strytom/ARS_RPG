using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player_Controller : MonoBehaviour
{
    [Header("Move parametrs")]
    [SerializeField] private float _speed = 6.0f;          // Скорость бега
    [SerializeField] private float _rotationSpeed = 720f;  // Скорость разворота персонажа (градусов в секунду)

    private Rigidbody _rb;
    private InputSystem_Actions _inputActions;
    private Transform _cameraTransform;

    private Vector2 _moveInput;     // Направление WASD от игрока
    private Vector3 _moveDirection; // Финальный вектор движения в мире

    // Это свойство пригодится нам на следующем этапе, чтобы блокировать бег во время ударов
    public bool IsAttacking { get; set; } = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _inputActions = new InputSystem_Actions();
        
        // Кэшируем ссылку на трансформ главной камеры, чтобы не искать её каждый кадр
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        // Блокируем стандартное вращение физического тела по осям X и Z.
        // Персонаж не должен заваливаться на бок при столкновениях.
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.useGravity = true;
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void Update()
    {
        // 1. Считываем ввод перемещения WASD (Vector2) каждый кадр
        _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();

        // Вычисляем вектор движения на основе взгляда камеры
        CalculateMovementDirection();
    }

    private void FixedUpdate()
    {
        // 2. Двигаем и разворачиваем персонажа в физическом цикле
        Move();
        RotateTowardsMovement();
    }

    private void CalculateMovementDirection()
    {
        if (_cameraTransform == null || IsAttacking)
        {
            _moveDirection = Vector3.zero;
            return;
        }

        // Получаем плоские (horizontal) векторы направления камеры
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        // Складываем векторы на основе нажатых клавиш W/S (y) и A/D (x)
        _moveDirection = (camForward * _moveInput.y + camRight * _moveInput.x);

        // Нормализуем финальный вектор движения, чтобы бег по диагонали не был быстрее, чем по прямой
        if (_moveDirection.magnitude > 1.0f)
        {
            _moveDirection.Normalize();
        }
    }

    private void Move()
    {
        if (_moveDirection == Vector3.zero) return;

        // Вычисляем новую позицию персонажа на основе физики
        Vector3 newPosition = _rb.position + _moveDirection * _speed * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);
    }

    private void RotateTowardsMovement()
    {
        // Персонаж разворачивается только тогда, когда мы куда-то бежим и не атакуем
        if (_moveDirection == Vector3.zero || IsAttacking) return;

        // Создаем целевое вращение в сторону направления движения
        Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);

        // Плавно разворачиваем персонажа
        _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
    }
}