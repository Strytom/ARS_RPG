using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private readonly int JumpHash = Animator.StringToHash("Jump");
    private const float CrossFadeDuration = 0.1f;
    private Vector3 _momentum;
    public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Vector3 velocity = _stateMachine.Rigidbody.linearVelocity;

        _momentum = new Vector3(velocity.x, 0f, velocity.z) / Mathf.Max(_stateMachine.FreeLookSpeed, 0.01f);
        _stateMachine.ForceReceiver.Jump(_stateMachine.JumpForce);

        _stateMachine.Animator.CrossFadeInFixedTime(JumpHash, CrossFadeDuration);
    }

    public override void Exit()
    {
        
    }

    public override void Tick(float deltaTime)
    {
        Move(_momentum, deltaTime);
        if (_stateMachine.Rigidbody.linearVelocity.y <=  0f)
        {
            _stateMachine.SwitchState(new PlayerFallState(_stateMachine));
            return;
        }
        FaceMovementTarget(deltaTime);
    }
}
