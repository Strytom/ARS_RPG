using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateMachine : StateMachine
{
    [Header("Enemy State Machine References")]
    [field: SerializeField] public Animator Animator{ get; set; }
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public GameObject Player { get; private set; }
    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }
    [field: SerializeField] public NavMeshAgent Agent { get; private set; }
    [field: SerializeField] public WeaponDamage Weapon { get; private set; }
    [field: SerializeField] public Target Target { get; private set; }
    [field: SerializeField] public Ragdoll Ragdoll { get; private set; }

    [Header("Enemy Parameters")]
    [field: SerializeField] public float ChaseRange { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float RotationDamping { get; private set; }
    [field: SerializeField] public float AttackRange { get; private set; }

    [Header("Enemy Attacks List")]
    [field: SerializeField] public Attack[] Attack { get; private set; }
    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

        Agent.updatePosition = false;
        Agent.updateRotation = false;

        SwitchState(new EnemyIdleState(this));
    }
    private void OnEnable()
    {
        Health.OnDamageTaken += HandleDamageTaken;
        Health.OnDeath += HandleDeath;
    }
    private void OnDisable()
    {
        Health.OnDamageTaken -= HandleDamageTaken;
        Health.OnDeath -= HandleDeath;
    }

    private void HandleDamageTaken()
    {
        SwitchState(new EnemyImpactState(this));
    }

    private void HandleDeath()
    {
        SwitchState(new EnemyDeadState(this));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ChaseRange);
    }
}
