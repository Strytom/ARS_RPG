using UnityEngine;

public class EnemyAttackingState : EnemyBaseState
{
    private Attack _attack;
    public EnemyAttackingState(EnemyStateMachine stateMachine, int AttackIndex) : base(stateMachine)
    {
        _attack = _stateMachine.Attack[AttackIndex];
        _stateMachine.Weapon.SetAttack(_attack.Damage, _attack.AttackKnockback);
    }

    public override void Enter()
    {
        _stateMachine.Animator.CrossFadeInFixedTime(_attack.AnimationName, _attack.TransitionDuration);
        Debug.Log("Attack State Entered");
    }

    public override void Exit()
    {
        
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);
        FaceMovementTarget(deltaTime);
        if (GetNormalizedTime(_stateMachine.Animator, _attack.AnimationTag) >= 1)
        {
            _stateMachine.SwitchState(new EnemyChaseState(_stateMachine));
        }
    }
}
