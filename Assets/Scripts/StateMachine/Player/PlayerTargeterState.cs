using System;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerTargeterState : PlayerBaseState
{
    private readonly int TargetingBlendTreeHash = Animator.StringToHash("TargetingBlendTree");
    private readonly int TargetingForwardHash = Animator.StringToHash("TargetingForward");
    private readonly int TargetingRightHash = Animator.StringToHash("TargetingRight");

    private const float CrossFadeDuration = 0.1f;


    public PlayerTargeterState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        _stateMachine.Input.OnAttackEvent += OnAttack;
        _stateMachine.Input.OnTargetEvent += OnTarget;
        _stateMachine.Input.OnBlockEvent += OnBlock;
        _stateMachine.Input.OnDodgeEvent += OnDodge;
        _stateMachine.Input.OnJumpEvent += OnJump;

        _stateMachine.Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        _stateMachine.cineFreeLookCamera.Priority = 0;
        _stateMachine.cineTargetCamera.Priority = 1;

        _stateMachine.Animator.CrossFadeInFixedTime(TargetingBlendTreeHash, CrossFadeDuration);

        if (_stateMachine.Crosshair != null && _stateMachine.Targeter.CurrentTarget != null){
        _stateMachine.Crosshair.SetTarget(_stateMachine.Targeter.CurrentTarget.transform);
        }
    }
    public override void Tick(float deltaTime)
    {

        if (_stateMachine.Targeter.CurrentTarget == null)
        {
            _stateMachine.SwitchState(new PlayerFreeLookState(_stateMachine));
            return;
        }
        Movement(deltaTime);
        UpdateAnimator(deltaTime);
        FaceMovementTarget(deltaTime);
    }
    public override void Exit()
    {
        _stateMachine.Input.OnAttackEvent -= OnAttack;
        _stateMachine.Input.OnTargetEvent -= OnTarget;
        _stateMachine.Input.OnBlockEvent -= OnBlock;
        _stateMachine.Input.OnDodgeEvent -= OnDodge;
        _stateMachine.Input.OnJumpEvent -= OnJump;
        if (_stateMachine.Crosshair != null)
        {
        _stateMachine.Crosshair.ClearTarget();
        }
    }

    private void OnJump()
    {
        if (!_stateMachine.IsGrounded)
        return;

        _stateMachine.SwitchState(new PlayerJumpState(_stateMachine));
    }

    private void Movement(float deltaTime)
    {
        Vector3 _moveDirection = CalculateMovementDirection(deltaTime);

        Move(_moveDirection, deltaTime);
    }

    public void AlignFreeLookCameraWithCurrentView()
    {
        var orbitalFollow = _stateMachine.cineFreeLookCamera.GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow != null && _stateMachine.CameraTransform != null)
        {
            // Take the current camera rotation and convert it to the orbital follow's axes
            Vector3 camEuler = _stateMachine.CameraTransform.eulerAngles;

            // Convert horizontal angle to [-180, 180] range
            float horizontal = camEuler.y;
            if (horizontal > 180f) horizontal -= 360f;

            // Convert vertical angle to [-180, 180] range
            float vertical = camEuler.x;
            if (vertical > 180f) vertical -= 360f;

            // Transfer angles to OrbitalFollow axes
            orbitalFollow.HorizontalAxis.Value = horizontal;
            orbitalFollow.VerticalAxis.Value = vertical + 10f;
        }
    }
    private void UpdateAnimator(float deltaTime)
    {
        if (_stateMachine.Input.MovementValue.y == 0)
        {
            _stateMachine.Animator.SetFloat(TargetingForwardHash, 0, 0.1f, deltaTime);
        }
        else
        {
            float value = _stateMachine.Input.MovementValue.y > 0 ? 1f : -1f;
            _stateMachine.Animator.SetFloat(TargetingForwardHash, value, 0.1f, deltaTime);
        }

        if (_stateMachine.Input.MovementValue.x == 0)
        {
            _stateMachine.Animator.SetFloat(TargetingRightHash, 0, 0.1f, deltaTime);
        }
        else
        {
            float value = _stateMachine.Input.MovementValue.x > 0 ? 1f : -1f;
            _stateMachine.Animator.SetFloat(TargetingRightHash, value, 0.1f, deltaTime);
        }


    }
    private void OnAttack()
    {
        _stateMachine.SwitchState(new PlayerAttackState(_stateMachine, attackIndex: 0));
    }

    private void OnTarget()
    {
        
        AlignFreeLookCameraWithCurrentView();
        _stateMachine.Targeter.ClearTarget();
        _stateMachine.SwitchState(new PlayerFreeLookState(_stateMachine)); 
    
    }
    private void OnBlock()
    {
        _stateMachine.SwitchState(new PlayerBlockingState(_stateMachine));
    }
    private void OnDodge()
    {
        if (Time.time - _stateMachine.PreviousDodgeTime < _stateMachine.DodgeCooldown) { return; }

        _stateMachine.SetDodgeTime(Time.time);
        _stateMachine.SwitchState(new PlayerDodgeState(_stateMachine, dodgingDirectionInput: _stateMachine.Input.MovementValue));
    }

}
