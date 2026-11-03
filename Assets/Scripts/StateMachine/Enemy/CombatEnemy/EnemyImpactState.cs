using UnityEngine;

public class EnemyImpactState : EnemyBaseState
{
    private readonly int impactHash = Animator.StringToHash("Impact");
    private const float CrossFadeDuration = 0.1f;
    private float _duration = 0.45f; // Достаточно времени, чтобы отыграть сгиб от удара и отлететь

    public EnemyImpactState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        _stateMachine.Animator.CrossFadeInFixedTime(impactHash, CrossFadeDuration);
        Debug.Log("Impact State Entered");

    }

    public override void Exit()
    {
    }

    public override void Tick(float deltaTime)
    {
        // Пока идет импакт — отлетаем назад под действием ForceReceiver
        Move(deltaTime);

        _duration -= deltaTime;
        if (_duration <= 0f)
        {
            // Сразу переходим к активным действиям без паузы в Idle!
            if (IsInChaseRange())
            {
                _stateMachine.SwitchState(new EnemyChaseState(_stateMachine));
            }
            else
            {
                _stateMachine.SwitchState(new EnemyIdleState(_stateMachine));
            }
        }
    }
}