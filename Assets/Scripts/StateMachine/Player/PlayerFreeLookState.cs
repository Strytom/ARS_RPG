using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerFreeLookState : PlayerBaseState
{
    private readonly int FreeLookBlendTreeHash = Animator.StringToHash("FreeLookBlendTree");
    private readonly int FreeLookSpeedHash = Animator.StringToHash("FreeLookSpeed");
    private const float AnimationDampTime = 0.1f;
    private const float CrossFadeDuration = 0.1f;

    public PlayerFreeLookState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        _stateMachine.Input.OnAttackEvent += OnAttack;
        _stateMachine.Input.OnTargetEvent += OnTarget;
        _stateMachine.Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        _stateMachine.cineFreeLookCamera.Priority = 1;
        _stateMachine.cineTargetCamera.Priority = 0;    

        _stateMachine.Animator.CrossFadeInFixedTime(FreeLookBlendTreeHash, CrossFadeDuration);
    }

    private void OnAttack()
    {
        _stateMachine.SwitchState(new PlayerAttackState(_stateMachine, attackIndex: 0));
    }

    public override void Tick(float deltaTime)
    {
        Movement(deltaTime);
    }

    public override void Exit()
    {
        _stateMachine.Input.OnAttackEvent -= OnAttack;
        _stateMachine.Input.OnTargetEvent -= OnTarget;
    }

    private void Movement(float deltaTime)
    {
        Vector3 _moveDirection = CalculateMovementDirection();

        Move(_moveDirection, deltaTime);

        if (_moveDirection == Vector3.zero){ 
            _stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0f, AnimationDampTime, Time.deltaTime);
            return ;
        }

            _stateMachine.Animator.SetFloat(FreeLookSpeedHash, 1f, AnimationDampTime, Time.deltaTime);
            _stateMachine.transform.rotation = FaceMovementDirection(_moveDirection, deltaTime);
    }
    private Quaternion FaceMovementDirection(Vector3 moveDirection, float deltaTime)
    {
        return Quaternion.Lerp(_stateMachine.transform.rotation, 
        Quaternion.LookRotation(moveDirection), 
        _stateMachine.RotationDamping * deltaTime);
    }
    
     private void OnTarget()
    {
        if (!_stateMachine.Targeter.SelectTarget())
        { return; }

        _stateMachine.SwitchState(new PlayerTargeterState(_stateMachine));
    }

}
