using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player_Controller : MonoBehaviour
{
    [Header("Move parametrs")]
    [SerializeField] private float _speed = 6.0f;          // Running speed
    [SerializeField] private float _rotationSpeed = 720f;  // Character rotation speed (degrees per second)
    [SerializeField] private float _jumpForce = 10f;        // Force applied when jumping
    private Rigidbody _rb;
    private InputSystem_Actions _inputActions;
    private Transform _cameraTransform;

    private Vector2 _moveInput;     // WASD input direction from the player
    private Vector3 _moveDirection; // Final movement vector in world space

    // This property will be useful later to block movement while attacking
    public bool IsAttacking { get; set; } = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _inputActions = new InputSystem_Actions();
        
        // Cache the main camera transform so we don't search for it every frame
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        // Freeze default physics body rotation on the X and Z axes.
        // The character should not tip over during collisions.
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
        // 1. Read WASD movement input (Vector2) each frame
        _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();

        // Calculate the movement vector based on the camera's direction
        CalculateMovementDirection();
    }

    private void FixedUpdate()
    {
        Jump(); // Call the Jump method to handle jumping logic
        // 2. Move and rotate the character in the physics cycle
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

        // Get the flattened (horizontal) camera direction vectors
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        // Combine vectors based on W/S (y) and A/D (x) input
        _moveDirection = (camForward * _moveInput.y + camRight * _moveInput.x);

        // Normalize the final movement vector so diagonal movement isn't faster than straight movement
        if (_moveDirection.magnitude > 1.0f)
        {
            _moveDirection.Normalize();
        }
    }

    private void Jump()
    {
        if (_inputActions.Player.Jump.triggered && IsGrounded())
        {
            // Apply an upward force to the Rigidbody to make the character jump
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }
    private bool IsGrounded()
    {
        // Check if the character is grounded by casting a ray downwards
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    private void Move()
    {
        if (_moveDirection == Vector3.zero) return;

        // Calculate the new character position based on physics
        Vector3 newPosition = _rb.position + _moveDirection * _speed * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);
    }

    private void RotateTowardsMovement()
    {
        // The character only rotates when moving and not attacking
        if (_moveDirection == Vector3.zero || IsAttacking) return;

        // Create target rotation toward the movement direction
        Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);

        // Smoothly rotate the character
        _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
    }
}