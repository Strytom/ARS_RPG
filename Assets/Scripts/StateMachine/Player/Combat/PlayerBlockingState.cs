using System;
using UnityEngine;

public class PlayerBlockingState : PlayerBaseState
{
    private readonly int BlockHash = Animator.StringToHash("Block");
    private const float CrossFadeDuration = 0.1f;

    private float _previousFrameTime, _time=0.35f;
    private bool _hasAppliedBlock;


    public PlayerBlockingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        _stateMachine.Animator.CrossFadeInFixedTime(BlockHash, CrossFadeDuration);
        _stateMachine.Health.SetIsInvincible(true);
    }

    public override void Exit()
    {
        _stateMachine.Health.SetIsInvincible(false);
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);
        FaceMovementTarget(deltaTime);

        float normalizedTime = GetNormalizedTime(_stateMachine.Animator, "Block");

        if (normalizedTime >= _previousFrameTime && normalizedTime < 1f)
        {
            if (normalizedTime >= _time && !_hasAppliedBlock)
        {
            TryApplyBlock();
        }
        }
        else
        {
            ReturnToLocomotion();
        }

        _previousFrameTime = normalizedTime;
    }

    private void TryApplyBlock()
    {
        _hasAppliedBlock = true;
        Debug.Log("Applying block to player");
    }
}
