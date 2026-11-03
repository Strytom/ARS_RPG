using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    private readonly int FreeLookBlendTreeHash = Animator.StringToHash("LocoLookBlend");
    private readonly int FreeLookSpeedHash = Animator.StringToHash("Speed");
    private const float AnimationDampTime = 0.1f;
    private const float CrossFadeDuration = 0.1f;

    public EnemyIdleState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        
    }

    public override void Enter()
    {
        _stateMachine.Animator.CrossFadeInFixedTime(FreeLookBlendTreeHash, CrossFadeDuration);
        Debug.Log("Idle State Entered");

    }
    public override void Exit()
    {
        
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);
        _stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0f, AnimationDampTime, deltaTime);

        if (IsInChaseRange())
        {
            _stateMachine.SwitchState(new EnemyChaseState(_stateMachine));
            return;
        }
    }
}
