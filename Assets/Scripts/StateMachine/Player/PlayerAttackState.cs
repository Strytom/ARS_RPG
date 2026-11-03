using System;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private bool _isComboAttackQueued, _alreadyAppliedForce;
    private float _previousFrameTime;
    private Attack _attack;

    public PlayerAttackState(PlayerStateMachine stateMachine, int attackIndex) : base(stateMachine)
    {
        _attack = _stateMachine.Attack[attackIndex];
        _stateMachine.Weapon.SetAttack(_attack.Damage, _attack.AttackKnockback);
    }

    public override void Enter()
    {
        _stateMachine.Input.OnAttackEvent += OnCombatAttack;

        _stateMachine.Animator.CrossFadeInFixedTime(_attack.AnimationName, _attack.TransitionDuration);
    }

    private void OnCombatAttack()
    {
        _isComboAttackQueued = true;
    }

    public override void Exit()
    {
        _stateMachine.Input.OnAttackEvent -= OnCombatAttack;
    }

    public override void Tick(float deltaTime)
    {
        FaceMovementTarget(deltaTime);
        Move(deltaTime);
        
        float normalizedTime = GetNormalizedTime(_stateMachine.Animator, _attack.AnimationTag);

        if (normalizedTime >= _previousFrameTime && normalizedTime < 1f)
        {
            if(normalizedTime >= _attack.ForceTime)
            {
                TryApplyForce();
            }
            if (_isComboAttackQueued)
            {
                TryComboAttack(normalizedTime);
            }
        }
        else
        {
            if(_stateMachine.Targeter.CurrentTarget != null)
            {
                _stateMachine.SwitchState(new PlayerTargeterState(_stateMachine));
            }
            else
            {
                _stateMachine.SwitchState(new PlayerFreeLookState(_stateMachine));
            }
        }

        _previousFrameTime = normalizedTime;
    }

    private void TryApplyForce()
    {
        if (_alreadyAppliedForce) return;

        _stateMachine.ForceReceiver.AddForce(_stateMachine.transform.forward * _attack.Force);

        _alreadyAppliedForce = true;
    }

    private void TryComboAttack(float normalizedTime)
    {
        if(_attack.ComboStateIndex == -1) return;
        if(normalizedTime < _attack.ComboAttackTime) return;
        _stateMachine.SwitchState(new PlayerAttackState(_stateMachine, _attack.ComboStateIndex));
    }
}
