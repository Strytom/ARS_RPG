using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ForceReceiver : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private float drag = 0.3f;

    private Vector3 dampingVelocity;
    private Vector3 impact;

    public Vector3 Move => impact;
    

    private void Update()
    {
        impact = Vector3.SmoothDamp(impact, Vector3.zero, ref dampingVelocity, drag);
        if (_agent != null)
        {
            if (impact.sqrMagnitude < 0.2f * 0.2f)
            {
                impact = Vector3.zero;
                _agent.enabled = true;
            }
        }

    }

    public void AddForce(Vector3 force)
    {
        impact += force;
        if (_agent != null)
        {
            _agent.enabled = false;
        }
    }
    public void Jump(float force)
    {
       Vector3 velocity = _rb.linearVelocity;

    // Сохраняем горизонтальную скорость.
    // force / mass соответствует прибавке скорости от Impulse.
    velocity.y = force / _rb.mass;

    _rb.linearVelocity = velocity;
    }
}
