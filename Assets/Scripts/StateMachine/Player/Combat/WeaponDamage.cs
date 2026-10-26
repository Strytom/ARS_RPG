using System.Collections.Generic;
using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    private List<Collider> _alreadyHitObjects = new List<Collider>();
    [SerializeField] private Collider MyCollider;
    private float _damage, _knockbackForce;

    private void OnEnable()
    {
        _alreadyHitObjects.Clear(); // Clear the list when the weapon is enabled

    }

    private void OnTriggerEnter(Collider other)
    {
       if (other == MyCollider) { return; } // Ignore self-collision
       if (_alreadyHitObjects.Contains(other)) {return;} // Ignore if already hit

       _alreadyHitObjects.Add(other); // Add to the list of already hit objects

        if (other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(_damage); // Example damage value, you can modify this as needed
        }
         if(other.TryGetComponent<ForceReceiver>(out ForceReceiver forceReceiver))
        {
            Vector3 direction = (other.transform.position - transform.position).normalized;
            direction.y = 0; // Optional: Keep the knockback force horizontal
            forceReceiver.AddForce(direction * _knockbackForce);
        }

    }
    public void SetAttack(float damage, float knockbackForce)
    {
        _damage = damage;
        _knockbackForce = knockbackForce;
    }
}
