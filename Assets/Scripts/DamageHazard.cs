using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class DamageHazard : MonoBehaviour
{
    [Header("Настройки ловушки")]
    [SerializeField] private float _damage = 10f;          // Damage amount
    [SerializeField] private float _damageInterval = 0.5f; // Damage interval in seconds
    [SerializeField] private float _rotationSpeed = 120f;  // Hazard rotation speed for visuals

    // Dictionary to track the last hit time per object (to avoid damage spam)
    private Dictionary<IDamageable, float> _damageCooldowns = new Dictionary<IDamageable, float>();

    private void Update()
    {
        // For visibility, the hazard constantly rotates in the scene
        transform.Rotate(Vector3.up * _rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerStay(Collider other)
    {
        // Check if the entering object implements IDamageable
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            if (!_damageCooldowns.ContainsKey(damageable))
            {
                _damageCooldowns.Add(damageable, 0f);
            }

            // Deal damage only if enough time has passed (interval)
            if (Time.time >= _damageCooldowns[damageable])
            {
                // Push the player away from the center of the hazard
                Vector3 pushDir = (other.transform.position - transform.position).normalized * 5f;
                
                damageable.TakeDamage(_damage, pushDir);
                _damageCooldowns[damageable] = Time.time + _damageInterval;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Remove the object from tracking when it leaves the hazard
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && _damageCooldowns.ContainsKey(damageable))
        {
            _damageCooldowns.Remove(damageable);
        }
    }
}
