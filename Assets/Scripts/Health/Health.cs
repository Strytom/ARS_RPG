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
    public bool IsDead => _currentHP <= 0;

    // Events for updating UI and animations (Observer Pattern)
    public event Action<float, float> OnHealthChanged; // Passes (current HP, max HP)
    public event Action OnDamageTaken, OnDeath; 

    private void Start()
    {
        // Invoke the event on start so UI shows full health immediately
        _currentHP = _maxHP;
        OnHealthChanged?.Invoke(_currentHP, _maxHP);
    }

    public void DealDamage(float amount)
    {
    // Мёртвые персонажи и неуязвимые не получают урон.
        if (IsDead || _isInvincible || amount <= 0f) return;

        _currentHP = Mathf.Max(_currentHP - amount, 0f);

        OnHealthChanged?.Invoke(_currentHP, _maxHP);

    // Проверяем смерть ПОСЛЕ уменьшения здоровья.
        if (IsDead){
        OnDeath?.Invoke();
        return;
        }

    // Реакция на удар — только если персонаж выжил.
        OnDamageTaken?.Invoke();
        StartCoroutine(DamageFeedbackRoutine());
        
    }

    private IEnumerator DamageFeedbackRoutine()
    {
        // Wait the remaining invincibility time
        float remainingTime = _invincibilityDuration - 0.15f;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

    }
    public void SetIsInvincible(bool value)
    {
        _isInvincible = value;
    }
}