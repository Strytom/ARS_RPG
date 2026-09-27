using System;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private float _previousFrameTime;
    private Attack _attack;

    public PlayerAttackState(PlayerStateMachine stateMachine, int attackIndex) : base(stateMachine)
    {
        _attack = _stateMachine.Attack[attackIndex];
    }

    public override void Enter()
    {
        _stateMachine.Animator.CrossFadeInFixedTime(_attack.AnimationName, _attack.TransitionDuration);
    }

    public override void Exit()
    {
        
    }

    public override void Tick(float deltaTime)
    {
        FaceMovementTarget(deltaTime);
        Move(deltaTime);
        float normalizedTime = GetNormalizedTime();

        if (normalizedTime > _previousFrameTime && normalizedTime < 1f)
        {
            if(_stateMachine.Input.IsAttacking)
            TryComboAttack(normalizedTime);
        }
        else if (normalizedTime >= 1f)
        {
            //_stateMachine.SwitchState(new PlayerFreeLookState(_stateMachine));
        }
        _previousFrameTime = normalizedTime;
    }

    private void TryComboAttack(float normalizedTime)
    {
        if(_attack.ComboStateIndex == -1) return;
        if(normalizedTime < _attack.ComboAttackTime) return;
        _stateMachine.SwitchState(new PlayerAttackState(_stateMachine, _attack.ComboStateIndex));
    }

    private float GetNormalizedTime()
    {
        AnimatorStateInfo currentAnimatorStateInfo = _stateMachine.Animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextAnimatorStateInfo = _stateMachine.Animator.GetNextAnimatorStateInfo(0);

        if (_stateMachine.Animator.IsInTransition(0) &&  nextAnimatorStateInfo.IsTag("Attack"))
        {
            return nextAnimatorStateInfo.normalizedTime;
        }
        else if (! _stateMachine.Animator.IsInTransition(0) && currentAnimatorStateInfo.IsTag("Attack"))
        {
            return currentAnimatorStateInfo.normalizedTime;
        }
        else
        {
            return 0f;
        }
        
    }
    
  
}
