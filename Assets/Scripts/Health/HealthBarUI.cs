using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Ссылки на UI элементы")]
    [SerializeField] private Slider _hpSlider;          // Ссылка на компонент Slider
    [SerializeField] private Text _hpText;              // Текст для отображения "HP: 100/100" (опционально)

    [Header("Ссылка на здоровье Игрока")]
    [SerializeField] private Health _playerHealth;      // Ссылка на компонент Health игрока

    private void Awake()
    {
        // Если игрок не назначен вручную в инспекторе, попытаемся найти его по тегу
        if (_playerHealth == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _playerHealth = player.GetComponent<Health>();
            }
        }
    }

    private void OnEnable()
    {
        if (_playerHealth != null)
        {
            // Подписываемся на событие изменения ХП
            _playerHealth.OnHealthChanged += UpdateHealthBar;
        }
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
        {
            // ОБЯЗАТЕЛЬНО отписываемся при выключении объекта, чтобы избежать утечек памяти!
            _playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(float currentHP, float maxHP)
    {
        if (_hpSlider != null)
        {
            // Слайдер в Unity работает от 0 до 1, поэтому передаем отношение здоровья
            _hpSlider.value = currentHP / maxHP;
        }

        if (_hpText != null)
        {
            _hpText.text = $"HP: {(int)currentHP} / {(int)maxHP}";
        }
    }
}
