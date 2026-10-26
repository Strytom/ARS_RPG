using UnityEngine;

public class PlayerImpactState : PlayerBaseState
{
   private readonly int impactHash = Animator.StringToHash("Impact");
   private float impactTime = 0.5f;
    public PlayerImpactState(PlayerStateMachine stateMachine) : base(stateMachine)
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
            ReturnToLocomotion();
        }
    }
}
