using System;
using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    private readonly int FallHash = Animator.StringToHash("Fall");
    private const float CrossFadeDuration = 0.1f;
    private Vector3 _momentum;
    public PlayerFallState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Vector3 velocity = _stateMachine.Rigidbody.linearVelocity;

        _momentum = new Vector3(velocity.x, 0f, velocity.z) / Mathf.Max(_stateMachine.FreeLookSpeed, 0.01f);
        _stateMachine.Animator.CrossFadeInFixedTime(FallHash, CrossFadeDuration);

        _stateMachine.LedgeDetector.OnLedgeDetected += HandleLedgeDetected;
    }

    public override void Exit()
    {
         _stateMachine.LedgeDetector.OnLedgeDetected -= HandleLedgeDetected;

    }

    private void HandleLedgeDetected(Vector3 closestPoint, Vector3 ledgeForward)
    {
        Debug.Log("Ledge Detected in Fall State");
        _stateMachine.SwitchState(new PlayerHangingState(_stateMachine, closestPoint, ledgeForward));
    }

    public override void Tick(float deltaTime)
    {
        if (_stateMachine.IsGrounded && _stateMachine.Rigidbody.linearVelocity.y <= 0.1f)
        {
        ReturnToLocomotion();
        return;
        }

        Move(_momentum, deltaTime);
        FaceMovementTarget(deltaTime);
    }
}
