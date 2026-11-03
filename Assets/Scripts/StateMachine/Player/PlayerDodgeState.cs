using UnityEngine;

public class PlayerDodgeState : PlayerBaseState
{
    private readonly int DodgeBlendTreeHash = Animator.StringToHash("DodgeBlendTree");
    private readonly int DodgeForwardHash = Animator.StringToHash("DodgeForward");
    private readonly int DodgeRightHash = Animator.StringToHash("DodgeRight");
    private const float CrossFadeDuration = 0.1f;

    private Vector3 dodgingDirectionInput;
    private float remainingDodgeTime;

    public PlayerDodgeState(PlayerStateMachine stateMachine, Vector3 dodgingDirectionInput) : base(stateMachine)
    {
        this.dodgingDirectionInput = dodgingDirectionInput;
    }

    public override void Enter()
    {
        remainingDodgeTime = _stateMachine.DodgeDuration;

        if (dodgingDirectionInput.sqrMagnitude < 0.01f)
        dodgingDirectionInput = Vector2.down;

        dodgingDirectionInput.Normalize();

        _stateMachine.Animator.SetFloat(DodgeForwardHash, dodgingDirectionInput.y);
        _stateMachine.Animator.SetFloat(DodgeRightHash, dodgingDirectionInput.x);
        _stateMachine.Animator.CrossFadeInFixedTime(DodgeBlendTreeHash, CrossFadeDuration);

        _stateMachine.Health.SetIsInvincible(true);
    }

    public override void Exit()
    {
        _stateMachine.Health.SetIsInvincible(false);

    }

    public override void Tick(float deltaTime)
    {
        Vector3 moveDirection = Vector3.zero;
        Vector3 direction =
        _stateMachine.transform.right * dodgingDirectionInput.x +
        _stateMachine.transform.forward * dodgingDirectionInput.y;

        float dodgeSpeed = _stateMachine.DodgeLength / Mathf.Max(_stateMachine.DodgeDuration, 0.01f);

        moveDirection = direction.normalized * dodgeSpeed / Mathf.Max(_stateMachine.FreeLookSpeed, 0.01f);

        Move(moveDirection, deltaTime);
        FaceMovementTarget(deltaTime);

        remainingDodgeTime = Mathf.Max(remainingDodgeTime - deltaTime, 0f);
        if (remainingDodgeTime <= 0f)
        {
            ReturnToLocomotion();
        }
    }
}
