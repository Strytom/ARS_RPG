using System;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class CombatDummy : MonoBehaviour, IDamageable
{
    [Header("Эффект шатания (Wobble)")]
    [SerializeField] private float _wobbleSpeed = 20f;   // Wobble speed
    [SerializeField] private float _wobbleDecay = 3f;    // Wobble decay speed

    private Quaternion _originalRotation;
    private Coroutine _wobbleCoroutine;

    private void Awake()
    {
        _originalRotation = transform.rotation;
    }

    public void TakeDamage(float amount, Vector3 pushDirection)
    {
        // 1. Create a floating damage text above the dummy
        // If the damage is high (heavy attack), highlight it orange; otherwise white
        Color textColor = amount > 15f ? new Color(1f, 0.5f, 0f) : Color.white;
        DamagePopup.Create(transform.position, (int)amount, textColor);

        Debug.Log($"Dummy {gameObject.name} took {amount} damage!");

        // 2. Start the wobble effect from the hit
        if (_wobbleCoroutine != null)
        {
            StopCoroutine(_wobbleCoroutine);
        }
        
        // The wobble direction depends on the hit vector
        Vector3 wobbleAxis = Vector3.Cross(Vector3.up, pushDirection).normalized;
        _wobbleCoroutine = StartCoroutine(WobbleRoutine(wobbleAxis, pushDirection.magnitude));
    }

    private IEnumerator WobbleRoutine(Vector3 axis, float force)
    {
        float angle = force * _wobbleSpeed; // The stronger the hit, the larger the tilt angle

        while (Mathf.Abs(angle) > 0.1f)
        {
            // Calculate the decaying sinusoidal tilt
            transform.rotation = _originalRotation * Quaternion.AngleAxis(angle, axis);
            
            // Smoothly reduce the tilt angle over time
            angle = Mathf.Lerp(angle, 0, _wobbleDecay * Time.deltaTime);
            yield return null;
        }

        transform.rotation = _originalRotation; // Restore original rotation
    }
}
