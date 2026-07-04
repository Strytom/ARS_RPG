using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Player_Controller : MonoBehaviour
{
    [Header("Move parametrs")]
    [SerializeField] private float _speed = 6.0f;          // Running speed
    [SerializeField] private float _rotationSpeed = 720f;  // Character rotation speed (degrees per second)
    [SerializeField] private float _jumpForce = 10f;        // Force applied when jumping
    [Header("Battle Parameters")]
    [SerializeField] private Transform _attackPoint;       // Point in front of the player from which the attack is calculated
    [SerializeField] private float _attackRadius = 1f;     // Radius of the damage zone
    [SerializeField] private LayerMask _damageableLayer;    // Layer on which enemies/mannequins are located

    private Rigidbody _rb;
    private InputSystem_Actions _inputActions;
    private Transform _cameraTransform;

    private Vector2 _moveInput;     // WASD input direction from the player
    private Vector3 _moveDirection; // Final movement vector in world space
    private float _cnockbackForce = 100f; // Force applied when taking damage

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
        _rb.constraints = RigidbodyConstraints.FreezeRotationX |RigidbodyConstraints.FreezeRotationY| RigidbodyConstraints.FreezeRotationZ;
        _rb.useGravity = true;

        // If the attack point isn't assigned manually, create it slightly in front of the player
        if (_attackPoint == null)
        {
            GameObject ap = new GameObject("AttackPoint");
            ap.transform.SetParent(transform);
            ap.transform.localPosition = new Vector3(0, 0, 1.2f);
            _attackPoint = ap.transform;
        }
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.Player.TestDamage.performed += OnTestDamagePressed;
        // Subscribe to attack inputs
        _inputActions.Player.Attack.performed += OnLightAttack;
        _inputActions.Player.HeavyAttack.performed += OnHeavyAttack;
    }

    private void OnDisable()
    {
        _inputActions.Disable();
        _inputActions.Player.TestDamage.performed -= OnTestDamagePressed;
        _inputActions.Player.Attack.performed -= OnLightAttack;
        _inputActions.Player.HeavyAttack.performed -= OnHeavyAttack;
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
    // --- Battle Logic ---

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;
        StartCoroutine(AttackRoutine(damage: 10f, lockDuration: 0.25f, pushForce: 6f, color: Color.cyan, scaleMultiplier: 1f));
    }

    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;
        StartCoroutine(AttackRoutine(damage: 25f, lockDuration: 0.5f, pushForce: 15f, color: Color.red, scaleMultiplier: 2f));
    }

    private IEnumerator AttackRoutine(float damage, float lockDuration, float pushForce, Color color, float scaleMultiplier)
    {
        IsAttacking = true;
        _rb.linearVelocity = Vector3.zero; // Reset running inertia

        // 1. Instant turn to face the camera's look direction
        if (_cameraTransform != null)
        {
            Vector3 lookDir = _cameraTransform.forward;
            lookDir.y = 0; // We only need horizontal rotation
            if (lookDir != Vector3.zero)
            {
                _rb.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        // 2. Create a visual swing placeholder (a simple expanding translucent cube in front of us)
        SpawnSlashPlaceholder(color, scaleMultiplier);

        // 3. Hit detection
        // Find all colliders inside our attack zone
        Collider[] hitColliders = Physics.OverlapSphere(_attackPoint.position, _attackRadius * scaleMultiplier, _damageableLayer);

        foreach (Collider col in hitColliders)
        {
            // Make sure we do not hit ourselves
            if (col.gameObject == gameObject) continue;

            // Look for the IDamageable interface on the object
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Calculate the push vector (push the enemy away)
                Vector3 pushDirection = (col.transform.position - transform.position).normalized * pushForce;
                
                // Deal damage through the interface!
                damageable.TakeDamage(damage, pushDirection);
            }
        }

        // Delay (attack animation and movement lock)
        yield return new WaitForSeconds(lockDuration);
        IsAttacking = false;
    }
    private void SpawnSlashPlaceholder(Color color, float scaleMultiplier)
    {
        // Create a temporary cube simulating a sword swing arc
        GameObject slash = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(slash.GetComponent<BoxCollider>()); // Remove the collider so it doesn't interfere with physics

        slash.transform.SetParent(transform);
        slash.transform.position = _attackPoint.position;
        slash.transform.rotation = transform.rotation;
        
        // Make it flat and wide (like a swing arc)
        slash.transform.localScale = new Vector3(2.5f * scaleMultiplier, 0.2f, 0.8f);

        // Configure color and material
        Renderer r = slash.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = color;
        }

        // Destroy this visual placeholder after 0.15 seconds (quick flash effect)
        Destroy(slash, 0.15f);
    }

    private void OnTestDamagePressed(InputAction.CallbackContext context)
    {
    // Look for a Health component on ourselves
    Health health = GetComponent<Health>();
    if (health != null)
    {
        // Simulate a frontal hit: push the character backward relative to its facing
        Vector3 pushDirection = -transform.forward * _cnockbackForce;
        health.TakeDamage(15f, pushDirection); // Deal 15 damage
        Debug.Log("Test damage dealt to the player!");
    }
    }
    // Visualize the attack radius in the Unity editor for easier tuning
    private void OnDrawGizmosSelected()
    {
        if (_attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_attackPoint.position, _attackRadius);
    }
}