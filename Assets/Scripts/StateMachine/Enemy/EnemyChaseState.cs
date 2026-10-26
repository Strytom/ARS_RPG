using System;
using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    private readonly int LocoBlendTreeHash = Animator.StringToHash("LocoLookBlend");
    private readonly int FreeLookSpeedHash = Animator.StringToHash("Speed");
    private const float AnimationDampTime = 0.1f;
    private const float CrossFadeDuration = 0.1f;

    public EnemyChaseState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        
    }

    public override void Enter()
    {
        _stateMachine.Animator.CrossFadeInFixedTime(LocoBlendTreeHash, 0.1f);
    }

    public override void Exit()
    {
        _stateMachine.Agent.ResetPath();
        _stateMachine.Rigidbody.linearVelocity = Vector3.zero;
    }

    public override void Tick(float deltaTime)
    {
        if (!IsInChaseRange())
        {
            _stateMachine.SwitchState(new EnemyIdleState(_stateMachine));
            return;
        }
        else if (IsInAttackRange())
        {
            _stateMachine.SwitchState(new EnemyAttackingState(_stateMachine, 0));
            return;
        }

        MoveToPlayer(deltaTime);

        FaceMovementTarget(deltaTime);

    // Считаем реальную горизонтальную скорость движения паладина (без оси Y)
        Vector3 horizontalVelocity = new Vector3(_stateMachine.Rigidbody.linearVelocity.x, 0, _stateMachine.Rigidbody.linearVelocity.z);
    
    // Если паладин движется (скорость > 0.2 м/с) — стремимся к 1f (Run), иначе к 0f (Idle)
        float targetSpeedAnim = horizontalVelocity.sqrMagnitude > 0.05f ? 1f : 0f;

        _stateMachine.Animator.SetFloat(FreeLookSpeedHash, targetSpeedAnim, AnimationDampTime, deltaTime);
    }

    private bool IsInAttackRange()
    {
        if (_stateMachine.Player == null) return false;
        float distance = Vector3.Distance(_stateMachine.Player.transform.position, _stateMachine.transform.position);
        return distance <= _stateMachine.AttackRange;
    }

    private void MoveToPlayer(float deltaTime)
    {
        if (_stateMachine.Agent.isOnNavMesh){
       // 1. Указываем цель (проецируя на плоскость, если игрок подпрыгнул)
        if (_stateMachine.Player != null){
        _stateMachine.Agent.SetDestination(_stateMachine.Player.transform.position);
        }

    // 2. Если путь ещё строится или агент остановился, не двигаем
        Vector3 desiredDir = _stateMachine.Agent.desiredVelocity;
    
    // Если desiredVelocity пока 0, но путь есть, берём направление на следующий угол пути (steeringTarget)
        if (desiredDir.sqrMagnitude < 0.01f && _stateMachine.Agent.hasPath){
        desiredDir = (_stateMachine.Agent.steeringTarget - _stateMachine.transform.position);
        }
        desiredDir.y = 0f;

    if (desiredDir.sqrMagnitude > 0.05f)
    {
        Vector3 moveDir = desiredDir.normalized;
        Move(moveDir, deltaTime);

       FaceMovementTarget(deltaTime);
    }
    else
    {
        Move(Vector3.zero, deltaTime);
    }
        }

    // 3. Синхронизируем положение виртуального агента с реальным Rigidbody
    //_stateMachine.Agent.nextPosition = _stateMachine.transform.position;
    _stateMachine.Agent.velocity = _stateMachine.Rigidbody.linearVelocity;
    }
    private void MoveToPlayere(float deltaTime)
    {
        if (_stateMachine.Agent.isOnNavMesh)
        {
            _stateMachine.Agent.destination = _stateMachine.Player.transform.position;

            Move(_stateMachine.Agent.desiredVelocity.normalized * _stateMachine.Speed, deltaTime);
        }

        _stateMachine.Agent.velocity = _stateMachine.Rigidbody.linearVelocity;
    }


}
