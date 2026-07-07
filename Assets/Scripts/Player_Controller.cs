using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player_Controller : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float _speed = 6.0f;
    [SerializeField] private float _rotationSpeed = 720f;

    [Header("Combat Parameters")]
    [SerializeField] private float _attackRadius = 1f;     // Base hit sphere radius
    [SerializeField] private LayerMask _damageableLayer;    // Enemy layer

    [Header("Attack Trajectory Settings (Local Offsets)")]
    // Light attack: strikes quickly, directly in front
    [SerializeField] private Vector3 _lightAttackStart = new Vector3(0f, 1.0f, 0.5f); // Point A (local)
    [SerializeField] private Vector3 _lightAttackEnd = new Vector3(0f, 1.0f, 4f);   // Point B (local)

    // Heavy attack: strikes farther and wider
    [SerializeField] private Vector3 _heavyAttackStart = new Vector3(0f, 1.0f, 0.8f); // Point A (local)
    [SerializeField] private Vector3 _heavyAttackEnd = new Vector3(0f, 1.0f, 8.0f);   // Point B (local)

    private Rigidbody _rb;
    private InputSystem_Actions _inputActions;
    private Transform _cameraTransform;

    private Vector2 _moveInput;
    private Vector3 _moveDirection;

    public bool IsAttacking { get; set; } = false;

    // Helper variables for debugging gizmos in the scene
    private Vector3 _debugPointA;
    private Vector3 _debugPointB;
    private float _debugRadius;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _inputActions = new InputSystem_Actions();
        
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        _rb.constraints = RigidbodyConstraints.FreezeRotationX | 
        RigidbodyConstraints.FreezeRotationY | 
        RigidbodyConstraints.FreezeRotationZ;
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
        if (_moveDirection == Vector3.zero || IsAttacking) return;

        Vector3 newPosition = _rb.position + _moveDirection * _speed * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);
    }

    private void RotateTowardsMovement()
    {
        if (_moveDirection == Vector3.zero || IsAttacking) return;

        Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
        _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
    }

    // --- COMBAT LOGIC ---

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;
        StartCoroutine(AttackTrajectoryRoutine(
            localStart: _lightAttackStart,
            localEnd: _lightAttackEnd,
            damage: 10f,
            lockDuration: 0.25f,
            pushForce: 6f,
            color: Color.cyan,
            radiusMultiplier: 1.0f
        ));
    }

    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;
        StartCoroutine(AttackTrajectoryRoutine(
            localStart: _heavyAttackStart,
            localEnd: _heavyAttackEnd,
            damage: 25f,
            lockDuration: 0.50f,
            pushForce: 15f,
            color: Color.red,
            radiusMultiplier: 1.5f
        ));
    }

    private IEnumerator AttackTrajectoryRoutine(Vector3 localStart, Vector3 localEnd, float damage, float lockDuration, float pushForce, Color color, float radiusMultiplier)
    {
        IsAttacking = true;
        _rb.linearVelocity = Vector3.zero; 

        // 1. Instantly rotate the character toward the camera look direction
        if (_cameraTransform != null)
        {
            Vector3 lookDir = _cameraTransform.forward;
            lookDir.y = 0; 
            if (lookDir != Vector3.zero)
            {
                _rb.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        // 2. TAKE A SPACE SNAPSHOT
        // Convert local start and end coordinates to world points
        Vector3 worldPointA = transform.TransformPoint(localStart);
        Vector3 worldPointB = transform.TransformPoint(localEnd);

        // Save for debug drawing in the editor
        _debugPointA = worldPointA;
        _debugPointB = worldPointB;
        _debugRadius = _attackRadius * radiusMultiplier;

        // 3. Lunge - apply physical force at the point
        Vector3 lungeForce = _cameraTransform.forward * (pushForce * 0.35f); 
        _rb.AddForce(lungeForce, ForceMode.Impulse);

        HashSet<IDamageable> hitTargetsThisSwing = new HashSet<IDamageable>();
        
        float activeDamageDuration = 0.15f; 
        float timer = 0f;

        // Create a moving visual wave placeholder
        GameObject slashVisual = SpawnSlashWave(worldPointA, color, radiusMultiplier);

        // Active damage phase
        while (timer < activeDamageDuration)
        {
            // Find attack progress from 0 to 1
            float progress = timer / activeDamageDuration;

            // MATH LERP: Find the current hitbox coordinate along line A->B
            Vector3 currentHitCenter = Vector3.Lerp(worldPointA, worldPointB, progress);

            // Sync the visual object with the hitbox point
            if (slashVisual != null)
            {
                slashVisual.transform.position = currentHitCenter;
            }

            // Scan space at this dynamic point
            Collider[] hitColliders = Physics.OverlapSphere(currentHitCenter, _attackRadius * radiusMultiplier, _damageableLayer);

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

        // Smoothly destroy the visual object at the end of the active phase
        if (slashVisual != null)
        {
            Destroy(slashVisual);
        }

        // Recovery delay for the character
        float remainingLock = lockDuration - activeDamageDuration;
        if (remainingLock > 0)
        {
            yield return new WaitForSeconds(remainingLock);
        }

        IsAttacking = false;
        _rb.linearVelocity = Vector3.zero; 
    }

    private GameObject SpawnSlashWave(Vector3 startPos, Color color, float scaleMultiplier)
    {
        // Create a temporary sphere indicator for the wave
        GameObject slash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(slash.GetComponent<Collider>()); // Remove the collider

        slash.transform.position = startPos;
        slash.transform.localScale = Vector3.one * (_attackRadius * 2f * scaleMultiplier);

        Renderer r = slash.GetComponent<Renderer>();
        if (r != null)
        {
            // Make the material semi-transparent for an energy effect
            r.material.color = new Color(color.r, color.g, color.b, 0.4f);
            
            // Set the rendering mode to transparent (if using the standard shader)
            r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            r.material.SetInt("_ZWrite", 0);
            r.material.DisableKeyword("_ALPHATEST_ON");
            r.material.EnableKeyword("_ALPHABLEND_ON");
            r.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            r.material.renderQueue = 3000;
        }

        return slash;
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

    // Draw the last attack trajectory directly in the Scene view
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw the A->B line
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(_debugPointA, _debugPointB);

        // Draw the start sphere
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Green start
        Gizmos.DrawWireSphere(_debugPointA, _debugRadius);

        // Draw the end sphere
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Red finish
        Gizmos.DrawWireSphere(_debugPointB, _debugRadius);
    }
}