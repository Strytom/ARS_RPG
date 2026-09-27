using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerFreeLookState : PlayerBaseState
{
    private readonly int FreeLookBlendTreeHash = Animator.StringToHash("FreeLookBlendTree");
    private readonly int FreeLookSpeedHash = Animator.StringToHash("FreeLookSpeed");
    private const float AnimationDampTime = 0.1f;

    public PlayerFreeLookState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        _stateMachine.Input.OnTargetEvent += OnTarget;
        _stateMachine.Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        _stateMachine.cineFreeLookCamera.Priority = 1;
        _stateMachine.cineTargetCamera.Priority = 0;    

        _stateMachine.Animator.Play(FreeLookBlendTreeHash);
    }

    public override void Tick(float deltaTime)
    {
        if (_stateMachine.Input.IsAttacking)
        {
            _stateMachine.SwitchState(new PlayerAttackState(_stateMachine, attackIndex: 0));
            return;
        }
         Movement(deltaTime);
    }

    public override void Exit()
    {
        _stateMachine.Input.OnTargetEvent -= OnTarget;
    }

    private void Movement(float deltaTime)
    {
        Vector3 _moveDirection = CalculateMovementDirection();

        Move(_moveDirection, deltaTime);

        if (_moveDirection == Vector3.zero){ 
            _stateMachine.Rigidbody.linearVelocity = new Vector3(0f, _stateMachine.Rigidbody.linearVelocity.y, 0f);//Stop the player from moving when there is no input
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
