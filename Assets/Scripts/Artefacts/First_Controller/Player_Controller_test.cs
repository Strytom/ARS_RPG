using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player_Controller_test : MonoBehaviour
{
    // Attack trajectory types
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
    [Tooltip("Start and end for linear trajectories")]
    [SerializeField] private Vector3 _linearStart = new Vector3(0f, 1.0f, 0.5f);
    [SerializeField] private Vector3 _linearEnd = new Vector3(0f, 1.0f, 3.5f);

    // --- Combo system step ---
    [Serializable]
    public struct ComboStep
    {
        public AttackPattern pattern;
        public float damage;
        public float lockDuration;
        public float pushForce;
        public float distance;
        public float angleRange;
        public Color color;
        public float radiusScale;
    }

    [Header("Combo Settings")]
    [SerializeField] private List<ComboStep> _comboSteps = new List<ComboStep>();
    [SerializeField] private float _comboResetTime = 0.8f; // Time to reset combo if the player does not attack

    private Rigidbody _rb;
    private InputSystem_Actions _inputActions;
    private Transform _cameraTransform;

    private Vector2 _moveInput;
    private Vector3 _moveDirection;

    public bool IsAttacking { get; set; } = false;

    // Combo system variables
    private int _currentComboIndex = 0;
    private bool _isNextAttackBuffered = false;
    private Coroutine _comboResetCoroutine;

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

        // If combo steps are not configured in the inspector, initialize 3 default strong hits
        if (_comboSteps == null || _comboSteps.Count == 0)
        {
            _comboSteps = new List<ComboStep>
            {
                // Hit 1: Slash left to right (Arc)
                new ComboStep { pattern = AttackPattern.Arc, damage = 10f, lockDuration = 0.3f, pushForce = 6f, distance = 3.0f, angleRange = 70f, color = Color.cyan, radiusScale = 0.5f },
                // Hit 2: Slash right to left (Arc, using negative -70 angle)
                new ComboStep { pattern = AttackPattern.Arc, damage = 12f, lockDuration = 0.3f, pushForce = 6f, distance = 3.0f, angleRange = -70f, color = Color.green, radiusScale = 0.5f },
                // Hit 3: Piercing lunge forward 4.5 meters (Linear) with strong push
                new ComboStep { pattern = AttackPattern.Linear, damage = 20f, lockDuration = 0.45f, pushForce = 12f, distance = 4.5f, angleRange = 0f, color = Color.red, radiusScale = 0.6f }
            };
        }
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

        // Reset all states when the object is disabled
        IsAttacking = false;
        _isNextAttackBuffered = false;
        _currentComboIndex = 0;
        if (_comboResetCoroutine != null)
        {
            StopCoroutine(_comboResetCoroutine);
            _comboResetCoroutine = null;
        }
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
            _rb.linearVelocity = new Vector3(_moveDirection.x * _speed * 0.8f, _rb.linearVelocity.y, _moveDirection.z * _speed * 0.8f);
        } else
        {
            _rb.linearVelocity = new Vector3(_moveDirection.x * _speed, _rb.linearVelocity.y, _moveDirection.z * _speed);
        }
    }

    private void Jump()
    {
        if (IsAttacking) return;
        if (_inputActions.Player.Jump.triggered && IsGrounded())
        {
            _rb.AddRelativeForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    private void RotateTowardsMovement()
    {
        if (_moveDirection == Vector3.zero || IsAttacking) return;

        if (_cameraTransform != null)
        {
            Vector3 camForward = _cameraTransform.forward;
            camForward.y = 0;

            if (camForward != Vector3.zero)
            {
                camForward.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(camForward);
                _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    // --- COMBAT SYSTEM (ATTACK PATTERNS) ---

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking)
        {
            // If the player pressed LMB during an attack — buffer the next combo step
            _isNextAttackBuffered = true;
            return;
        }

        // Stop the reset timer if it was started
        if (_comboResetCoroutine != null)
        {
            StopCoroutine(_comboResetCoroutine);
            _comboResetCoroutine = null;
        }

        // Start the combo chain
        StartCoroutine(ExecuteComboChain());
    }

    private IEnumerator ExecuteComboChain()
    {
        while (true)
        {
            _isNextAttackBuffered = false;

            if (_comboSteps == null || _comboSteps.Count == 0) break;

            ComboStep step = _comboSteps[_currentComboIndex];

            Debug.Log($"[Combo] Executing hit {(_currentComboIndex + 1)}/{_comboSteps.Count} ({step.pattern})");

            // Wait for the current attack coroutine to finish
            yield return StartCoroutine(AttackRoutine(
                pattern: step.pattern,
                damage: step.damage,
                lockDuration: step.lockDuration,
                pushForce: step.pushForce,
                distance: step.distance,
                angleRange: step.angleRange,
                color: step.color,
                radiusScale: step.radiusScale
            ));

            // If the player pressed LMB again during the attack — move to the next combo step
            if (_isNextAttackBuffered)
            {
                _currentComboIndex = (_currentComboIndex + 1) % _comboSteps.Count;
            }
            else
            {
                // If no click followed, start the combo reset coroutine by timer
                _comboResetCoroutine = StartCoroutine(ComboResetRoutine());
                break;
            }
        }
    }

    private IEnumerator ComboResetRoutine()
    {
        yield return new WaitForSeconds(_comboResetTime);
        _currentComboIndex = 0;
        _comboResetCoroutine = null;
        Debug.Log("[Combo] Combo chain has been reset.");
    }

    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;

        // Strong crushing ground slam (Ground Slam) — heavy attack outside the combo chain
        StartCoroutine(AttackRoutine(
            pattern: AttackPattern.GroundSlam,
            damage: 25f,
            lockDuration: 0.6f,
            pushForce: 15f,
            distance: 4.0f,            
            angleRange: 360f,          
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

        // 1. Turn toward the aim
        if (_cameraTransform != null)
        {
            Vector3 lookDir = _cameraTransform.forward;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                _rb.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        // Capture a snapshot of world space at the start of the strike
        Vector3 startPivot = transform.position;
        Vector3 localForwardSnapshot = new Vector3(_cameraTransform.forward.x, 0, _cameraTransform.forward.z).normalized;
        Vector3 worldPointA = transform.TransformPoint(_linearStart);
        Vector3 worldPointB = transform.TransformPoint(_linearEnd);

        // Impulse of the character lunge forward
        _rb.AddForce(transform.forward * (pushForce * 0.3f), ForceMode.Impulse);

        HashSet<IDamageable> hitTargetsThisSwing = new HashSet<IDamageable>();
        
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

            switch (pattern)
            {
                case AttackPattern.Linear:
                    currentHitCenter = Vector3.Lerp(worldPointA, worldPointB, progress);
                    break;

                case AttackPattern.Arc:
                    float currentArcAngle = Mathf.Lerp(-angleRange / 2f, angleRange / 2f, progress);
                    Vector3 arcDir = Quaternion.AngleAxis(currentArcAngle, Vector3.up) * localForwardSnapshot;
                    currentHitCenter = startPivot + (arcDir * distance) + (Vector3.up * _attackHeightOffset);
                    break;

                case AttackPattern.Whirlwind:
                    float currentSpinAngle = Mathf.Lerp(0f, 360f, progress);
                    Vector3 spinDir = Quaternion.AngleAxis(currentSpinAngle, Vector3.up) * localForwardSnapshot;
                    currentHitCenter = startPivot + (spinDir * distance) + (Vector3.up * _attackHeightOffset);
                    break;

                case AttackPattern.GroundSlam:
                    Vector3 slamCenter = startPivot + (localForwardSnapshot * distance) + (Vector3.up * _attackHeightOffset);
                    currentHitCenter = slamCenter;
                    currentScanRadius = Mathf.Lerp(0.1f, _attackRadius * radiusScale, progress);
                    break;

                case AttackPattern.Spiral:
                    float currentSpiralAngle = Mathf.Lerp(0f, angleRange, progress);
                    float currentSpiralDist = Mathf.Lerp(0.5f, distance, progress);
                    Vector3 spiralDir = Quaternion.AngleAxis(currentSpiralAngle, Vector3.up) * localForwardSnapshot;
                    currentHitCenter = startPivot + (spiralDir * currentSpiralDist) + (Vector3.up * _attackHeightOffset);
                    break;
            }

            _currentDebugCenter = currentHitCenter;
            _currentDebugRadius = currentScanRadius;

            if (slashVisual != null)
            {
                slashVisual.transform.position = currentHitCenter;
                slashVisual.transform.localScale = Vector3.one * (currentScanRadius * 2f);
            }

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

        _drawActiveGizmo = false;
        if (slashVisual != null) Destroy(slashVisual);

        // Recovery phase after the swing
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
        Destroy(visual.GetComponent<Collider>()); 

        Renderer r = visual.GetComponent<Renderer>();
        if (r != null)
        {
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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_drawActiveGizmo) return;

        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f); 
        Gizmos.DrawWireSphere(_currentDebugCenter, _currentDebugRadius);
        Gizmos.DrawSphere(_currentDebugCenter, _currentDebugRadius * 0.15f); 
    }
}
