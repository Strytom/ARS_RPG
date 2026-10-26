using UnityEngine;

public class EnemyImpactState : EnemyBaseState
{
    private readonly int impactHash = Animator.StringToHash("Impact");
    private float impactTime = 0.5f;
    public EnemyImpactState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        _stateMachine.Animator.CrossFadeInFixedTime(impactHash, 0.1f);
    }

    public override void Exit()
    {
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);
        impactTime -= deltaTime;
        if (impactTime <= 0)
        {
            _stateMachine.SwitchState(new EnemyIdleState(_stateMachine));
        }
    }
}
