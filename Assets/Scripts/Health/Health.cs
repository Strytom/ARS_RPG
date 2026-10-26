using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour 
{
    [Header("References")]

    [Header("Health parameters")]
    [SerializeField] private float _maxHP = 100f;
    private float _currentHP;

    [Header("Invincibility frames (i-frames)")]
    [SerializeField] private float _invincibilityDuration = 0.5f; // Duration of invincibility in seconds
    private bool _isInvincible = false;

    // Events for updating UI and animations (Observer Pattern)
    public event Action<float, float> OnHealthChanged; // Passes (current HP, max HP)
    public event Action OnDamageTaken; // Triggered when damage is taken

    private void Start()
    {
        // Invoke the event on start so UI shows full health immediately
        _currentHP = _maxHP;
        OnHealthChanged?.Invoke(_currentHP, _maxHP);
    }

    public void DealDamage(float amount)
    {
        if (_isInvincible || _currentHP == 0) return;

        _currentHP = Mathf.Max(_currentHP - amount, 0);

        Debug.Log($"Damage taken: {amount}. Current HP: {_currentHP}/{_maxHP}");

        // Notify subscribers (for example, the health bar UI)
        OnHealthChanged?.Invoke(_currentHP, _maxHP);

        // Notify subscribers (for example, the damage indicator UI)
        OnDamageTaken?.Invoke();

        // Start the invincibility coroutine and red flash
        StartCoroutine(DamageFeedbackRoutine());
    }

    private IEnumerator DamageFeedbackRoutine()
    {
        _isInvincible = true;

        // Wait the remaining invincibility time
        float remainingTime = _invincibilityDuration - 0.15f;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        _isInvincible = false;
    }

}