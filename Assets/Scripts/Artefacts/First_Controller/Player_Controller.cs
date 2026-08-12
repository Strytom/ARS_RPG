using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controller : MonoBehaviour
{
    // Declare attack trajectory types
    public enum AttackPattern { Linear, Arc, Whirlwind, GroundSlam, Spiral }

    [Header("Movement Parameters")]
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _rotationSpeed = 720f;
    [SerializeField] private float _jumpForce = 100.0f;

    [Header("Combat Parameters")]
    [SerializeField] private float _attackRadius = 1.2f;     // Base radius of the damage sphere
    [SerializeField] private LayerMask _damageableLayer;    // Enemy layer
    [SerializeField] private float _attackHeightOffset = 1.0f; // Height of the strike relative to the ground

    [Header("Specific Pattern Settings")]
    [Tooltip("Start and end points for linear trajectories")]
    [SerializeField] private Vector3 _linearStart = new Vector3(0f, 1.0f, 0.5f);
    [SerializeField] private Vector3 _linearEnd = new Vector3(0f, 1.0f, 3.5f);

    private Rigidbody _rb;
    private InputSystem_Actions _inputActions;
    private Transform _cameraTransform;

    private Vector2 _moveInput;
    private Vector3 _moveDirection;

    public bool IsAttacking { get; set; } = false;

    // --- Variables for dynamic debugging in the Scene View ---
    private Vector3 _currentDebugCenter;
    private float _currentDebugRadius;
    private bool _drawActiveGizmo = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _inputActions = new InputSystem_Actions();
        
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        _rb.useGravity = true;
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.Player.Attack.performed += OnLightAttack;
        _inputActions.Player.HeavyAttack.performed += OnHeavyAttack;
        _inputActions.Player.TestDamage.performed += OnTestDamagePressed;
    }

    private void OnDisable()
    {
        _inputActions.Player.Attack.performed -= OnLightAttack;
        _inputActions.Player.HeavyAttack.performed -= OnHeavyAttack;
        _inputActions.Player.TestDamage.performed -= OnTestDamagePressed;
        _inputActions.Disable();
    }

    private void Update()
    {
        _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
        CalculateMovementDirection();
    }

    private void FixedUpdate()
    {
        Jump();
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

        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        _moveDirection = (camForward * _moveInput.y + camRight * _moveInput.x);

        if (_moveDirection.magnitude > 1.0f)
        {
            _moveDirection.Normalize();
        }
    }

    private void Move()
    {
        if (_moveDirection == Vector3.zero || IsAttacking) {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
            return;
        }   
        if (!IsGrounded()) 
        {
            _rb.linearVelocity = new Vector3(_moveDirection.x * _speed * 0.5f, _rb.linearVelocity.y, _moveDirection.z * _speed * 0.5f);
        } else
        {
            _rb.linearVelocity = new Vector3(_moveDirection.x * _speed, _rb.linearVelocity.y, _moveDirection.z * _speed);
        }
    }
    private void Jump()
    {
        if(IsAttacking) return;
        if (_inputActions.Player.Jump.triggered && IsGrounded())
        {
            // Apply an upward force to the Rigidbody to make the character jump
            _rb.AddRelativeForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }
    private bool IsGrounded()
    {
        // Check if the character is grounded by casting a ray downwards
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    private void RotateTowardsMovement()
    {
        // If the player is not moving or is currently attacking, we don't want to change the rotation
        if (_moveDirection == Vector3.zero || IsAttacking) return;

        if (_cameraTransform != null)
        {
            // Take the forward direction of the camera as the reference for rotation
            Vector3 camForward = _cameraTransform.forward;
            
            // Project the camera's forward vector onto the horizontal plane
            // This ensures that the character doesn't tilt up or down when the camera is looking up or down
            camForward.y = 0;

            if (camForward != Vector3.zero)
            {
                camForward.Normalize();
                
                // Calculate the target rotation based on the camera's forward direction
                Quaternion targetRotation = Quaternion.LookRotation(camForward);
                
                // Smoothly rotate the character towards the target rotation
                _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    // --- COMBAT SYSTEM (ATTACK PATTERNS) ---

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;
        
        // LIGHT ATTACK: Arc Cleave (Semi-circular fan swing in front of the player)
        StartCoroutine(AttackRoutine(
            pattern: AttackPattern.Arc,
            damage: 10f,
            lockDuration: 0.3f,
            pushForce: 6f,
            distance: 3.0f,            // Distance of the semicircle
            angleRange: 70f,          // Angle of the swing
            color: Color.cyan,
            radiusScale: 0.5f
        ));
    }

    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;

        // HEAVY ATTACK: Ground Slam (Downward strike that hits in a circular area)
        StartCoroutine(AttackRoutine(
            pattern: AttackPattern.GroundSlam,
            damage: 25f,
            lockDuration: 0.6f,
            pushForce: 15f,
            distance: 4.0f,            // Maximum radius of the ground slam
            angleRange: 360f,          // Full rotation (360 degrees)
            color: Color.red,
            radiusScale: 1.3f
        ));
    }

    private IEnumerator AttackRoutine(
        AttackPattern pattern, 
        float damage, 
        float lockDuration, 
        float pushForce, 
        float distance, 
        float angleRange, 
        Color color, 
        float radiusScale)
    {
        IsAttacking = true;
        _rb.linearVelocity = Vector3.zero;

        // 1. Rotate towards the aim
        if (_cameraTransform != null)
        {
            Vector3 lookDir = _cameraTransform.forward;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                _rb.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        // Capture world start/turn points at the start of the strike (space snapshot)
        Vector3 startPivot = transform.position;
        Vector3 localForwardSnapshot = new Vector3(_cameraTransform.forward.x, 0, _cameraTransform.forward.z).normalized;
        Vector3 worldPointA = transform.TransformPoint(_linearStart);
        Vector3 worldPointB = transform.TransformPoint(_linearEnd);

        // Physical impulse pushing the player forward (for feel/dynamics)
        _rb.AddForce(transform.forward * (pushForce * 0.3f), ForceMode.Impulse);

        HashSet<IDamageable> hitTargetsThisSwing = new HashSet<IDamageable>();
        
        // Active damage phase simulation time
        float activeDamageDuration = 0.20f; 
        float timer = 0f;

        // Spawn visual indicator (energy sphere)
        GameObject slashVisual = SpawnVisualSphere(color, _attackRadius * radiusScale);

        _drawActiveGizmo = true;

        while (timer < activeDamageDuration)
        {
            float progress = timer / activeDamageDuration;
            Vector3 currentHitCenter = Vector3.zero;
            float currentScanRadius = _attackRadius * radiusScale;

            // --- Mathematical selection of trajectory ---
            switch (pattern)
            {
                case AttackPattern.Linear:
                    // Simple flight forward along the vector from A to B
                    currentHitCenter = Vector3.Lerp(worldPointA, worldPointB, progress);
                    break;

                case AttackPattern.Arc:
                    // Semi-circular swing. Interpolate angle from -HalfAngle to +HalfAngle
                    float currentArcAngle = Mathf.Lerp(-angleRange / 2f, angleRange / 2f, progress);
                    // Rotate the horizontal forward vector
                    Vector3 arcDir = Quaternion.AngleAxis(currentArcAngle, Vector3.up) * localForwardSnapshot;
                    currentHitCenter = startPivot + (arcDir * distance) + (Vector3.up * _attackHeightOffset);
                    break;

                case AttackPattern.Whirlwind:
                    // Circular strike around the player (360 degrees)
                    float currentSpinAngle = Mathf.Lerp(0f, 360f, progress);
                    Vector3 spinDir = Quaternion.AngleAxis(currentSpinAngle, Vector3.up) * localForwardSnapshot;
                    currentHitCenter = startPivot + (spinDir * distance) + (Vector3.up * _attackHeightOffset);
                    break;

                case AttackPattern.GroundSlam:
                    // Area slam at distance. Point is static but sphere radius expands
                    Vector3 slamCenter = startPivot + (localForwardSnapshot * distance) + (Vector3.up * _attackHeightOffset);
                    currentHitCenter = slamCenter;
                    currentScanRadius = Mathf.Lerp(0.1f, _attackRadius * radiusScale, progress);
                    break;

                case AttackPattern.Spiral:
                    // Spiral: the sphere spins around the player while moving outward
                    float currentSpiralAngle = Mathf.Lerp(0f, angleRange, progress);
                    float currentSpiralDist = Mathf.Lerp(0.5f, distance, progress);
                    Vector3 spiralDir = Quaternion.AngleAxis(currentSpiralAngle, Vector3.up) * localForwardSnapshot;
                    currentHitCenter = startPivot + (spiralDir * currentSpiralDist) + (Vector3.up * _attackHeightOffset);
                    break;
            }

            // Update debug variables
            _currentDebugCenter = currentHitCenter;
            _currentDebugRadius = currentScanRadius;

            // Synchronize visual effect
            if (slashVisual != null)
            {
                slashVisual.transform.position = currentHitCenter;
                slashVisual.transform.localScale = Vector3.one * (currentScanRadius * 2f);
            }

            // Scan the area at the computed point
            Collider[] hitColliders = Physics.OverlapSphere(currentHitCenter, currentScanRadius, _damageableLayer);
            foreach (Collider col in hitColliders)
            {
                if (col.gameObject == gameObject) continue;

                IDamageable damageable = col.GetComponent<IDamageable>();
                if (damageable != null && !hitTargetsThisSwing.Contains(damageable))
                {
                    hitTargetsThisSwing.Add(damageable);
                    Vector3 pushDirection = (col.transform.position - transform.position).normalized * pushForce;
                    damageable.TakeDamage(damage, pushDirection);
                }
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Cleanup visual after the strike
        _drawActiveGizmo = false;
        if (slashVisual != null) Destroy(slashVisual);

        // Recovery phase
        float remainingLock = lockDuration - activeDamageDuration;
        if (remainingLock > 0)
        {
            yield return new WaitForSeconds(remainingLock);
        }

        IsAttacking = false;
        _rb.linearVelocity = Vector3.zero;
    }

    private GameObject SpawnVisualSphere(Color color, float scaleRadius)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(visual.GetComponent<Collider>()); // Remove physical collider

        Renderer r = visual.GetComponent<Renderer>();
        if (r != null)
        {
            // Configure semi-transparent material to show the "strike energy"
            r.material.color = new Color(color.r, color.g, color.b, 0.4f);
            r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            r.material.SetInt("_ZWrite", 0);
            r.material.DisableKeyword("_ALPHATEST_ON");
            r.material.EnableKeyword("_ALPHABLEND_ON");
            r.material.renderQueue = 3000;
        }

        return visual;
    }

    private void OnTestDamagePressed(InputAction.CallbackContext context)
    {
        Health health = GetComponent<Health>();
        if (health != null)
        {
            Vector3 pushDirection = -transform.forward * 8f;
            health.TakeDamage(15f, pushDirection);
        }
    }

    // Dynamic drawing of the sphere in real-time in the Scene View during testing
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_drawActiveGizmo) return;

        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f); // Bright yellow semi-transparent color
        Gizmos.DrawWireSphere(_currentDebugCenter, _currentDebugRadius);
        Gizmos.DrawSphere(_currentDebugCenter, _currentDebugRadius * 0.15f); // Small core at the center of the hitbox
    }
}