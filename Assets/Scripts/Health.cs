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

        // Если рендерер не назначен вручную, пытаемся найти его на объекте
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
        // Вызываем событие при старте, чтобы UI сразу отобразил полное здоровье
        OnHealthChanged?.Invoke(_currentHP, _maxHP);
    }

    public void TakeDamage(float amount, Vector3 pushDirection)
    {
        // Если персонаж в данный момент неуязвим — урон игнорируется
        if (_isInvincible || _currentHP <= 0) return;

        _currentHP -= amount;
        _currentHP = Mathf.Clamp(_currentHP, 0, _maxHP);

        // Уведомляем всех подписчиков (например, шкалу здоровья в UI)
        OnHealthChanged?.Invoke(_currentHP, _maxHP);

        // Применяем физический импульс отталкивания, если есть Rigidbody
        if (_rb != null && pushDirection != Vector3.zero)
        {
            // Обнуляем вертикальную составляющую, чтобы персонаж не улетал в небо
            pushDirection.y = 0;
            // Применяем силу импульсом (ForceMode.Impulse идеально подходит для ударов)
            _rb.AddForce(pushDirection, ForceMode.Impulse);
        }

        // Проверяем смерть
        if (_currentHP <= 0)
        {
            Die();
        }
        else
        {
            // Запускаем корутину неуязвимости и мигания красным цветом
            StartCoroutine(DamageFeedbackRoutine());
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} погиб!");
        OnDeath?.Invoke();
        
        // В реальной игре здесь отключалось бы управление и запускалась анимация смерти.
        // Для прототипа мы пока просто выведем лог.
    }

    private IEnumerator DamageFeedbackRoutine()
    {
        _isInvincible = true;

        // Эффект окрашивания в красный цвет
        if (_characterRenderer != null && _characterRenderer.material.HasProperty("_Color"))
        {
            _characterRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.15f); // Держим красный цвет 150мс
            _characterRenderer.material.color = _originalColor;
        }

        // Ждем остаток времени неуязвимости
        float remainingTime = _invincibilityDuration - 0.15f;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        _isInvincible = false;
    }
}