using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health parameters")]
    [SerializeField] private float _maxHP = 100f;
    private float _currentHP;

    [Header("Invincibility frames (i-frames)")]
    [SerializeField] private float _invincibilityDuration = 0.5f; // Duration of invincibility in seconds
    private bool _isInvincible = false;

    [Header("Visual effects")]
    [SerializeField] private Renderer _characterRenderer; // Reference to the character's mesh for blinking
    private Color _originalColor;
    private Rigidbody _rb;

    // Events for updating UI and animations (Observer Pattern)
    public event Action<float, float> OnHealthChanged; // Passes (current HP, max HP)
    public event Action OnDeath;

    private void Awake()
    {
        _currentHP = _maxHP;
        _rb = GetComponent<Rigidbody>();

        // If the renderer is not assigned manually, try to find it on the object
        if (_characterRenderer == null)
        {
            _characterRenderer = GetComponentInChildren<Renderer>();
        }

        if (_characterRenderer != null && _characterRenderer.material.HasProperty("_Color"))
        {
            _originalColor = _characterRenderer.material.color;
        }
    }

    private void Start()
    {
        // Invoke the event on start so UI shows full health immediately
        OnHealthChanged?.Invoke(_currentHP, _maxHP);
    }

    public void TakeDamage(float amount, Vector3 pushDirection)
    {
        // If the character is currently invincible, damage is ignored
        if (_isInvincible || _currentHP <= 0) return;

        _currentHP -= amount;
        _currentHP = Mathf.Clamp(_currentHP, 0, _maxHP);

        // Notify subscribers (for example, the health bar UI)
        OnHealthChanged?.Invoke(_currentHP, _maxHP);

        // Apply physical knockback impulse if there is a Rigidbody
        if (_rb != null && pushDirection != Vector3.zero)
        {
            // Zero out the vertical component so the character doesn't fly upward
            pushDirection.y = 0;
            // Apply the force as an impulse (ForceMode.Impulse is ideal for hits)
            _rb.AddForce(pushDirection, ForceMode.Impulse);
        }

        // Check for death
        if (_currentHP <= 0)
        {
            Die();
        }
        else
        {
            // Start the invincibility coroutine and red flash
            StartCoroutine(DamageFeedbackRoutine());
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        OnDeath?.Invoke();
        
        // In a real game, control would be disabled and a death animation played here.
        // For the prototype, we just log it for now.
    }

    private IEnumerator DamageFeedbackRoutine()
    {
        _isInvincible = true;

        // Red tint effect
        if (_characterRenderer != null && _characterRenderer.material.HasProperty("_Color"))
        {
            _characterRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.15f); // Hold red color for 150ms
            _characterRenderer.material.color = _originalColor;
        }

        // Wait the remaining invincibility time
        float remainingTime = _invincibilityDuration - 0.15f;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        _isInvincible = false;
    }
}