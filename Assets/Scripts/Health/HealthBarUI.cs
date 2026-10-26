using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider _hpSlider;          // Reference to the Slider component
    [SerializeField] private Text _hpText;              // Text for displaying "HP: 100/100" (optional)

    [Header("Reference to Player's Health")]
    [SerializeField] private Health _playerHealth;      // Reference to the player's Health component

    private void Awake()
    {
        // If the player is not assigned manually in the inspector, try to find it by tag
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
            _playerHealth.OnHealthChanged += UpdateHealthBar;
        }
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(float currentHP, float maxHP)
    {
        if (_hpSlider != null)
        {
            _hpSlider.value = currentHP / maxHP;
        }

        if (_hpText != null)
        {
            _hpText.text = $"HP: {(int)currentHP} / {(int)maxHP}";
        }
    }
}
