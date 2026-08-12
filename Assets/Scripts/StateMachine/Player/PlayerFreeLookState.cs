using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerFreeLookState : PlayerBaseState
{
    private readonly int FreeLookSpeedHash = Animator.StringToHash("FreeLookSpeed");
    private const float AnimationDampTime = 0.1f;

    public PlayerFreeLookState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
       _stateMachine.Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public override void Tick(float deltaTime)
    {
         Move(deltaTime);
    }

    public override void Exit()
    {
        
    }

    private void Move(float deltaTime)
    {
        Vector3 _moveDirection = CalculateMovementDirection();

        _stateMachine.Rigidbody.linearVelocity = new Vector3(_moveDirection.x * _stateMachine.FreeLookSpeed, _stateMachine.Rigidbody.linearVelocity.y, _moveDirection.z * _stateMachine.FreeLookSpeed);


        if (_moveDirection == Vector3.zero){ 
            _stateMachine.Rigidbody.linearVelocity = new Vector3(0f, _stateMachine.Rigidbody.linearVelocity.y, 0f);//Stop the player from moving when there is no input
            _stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0f, AnimationDampTime, Time.deltaTime);
            return ;
            }

            _stateMachine.Animator.SetFloat(FreeLookSpeedHash, 1f, AnimationDampTime, Time.deltaTime);
            _stateMachine.transform.rotation = FaceMovementDirection(_moveDirection, deltaTime);
    }
    private Vector3 CalculateMovementDirection()
    {

        Vector3 camForward = _stateMachine.CameraTransform.forward;
        Vector3 camRight = _stateMachine.CameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 _moveDirection = (camForward * _stateMachine.Input.MovementValue.y + camRight * _stateMachine.Input.MovementValue.x);

        return _moveDirection;
    }

    private Quaternion FaceMovementDirection(Vector3 moveDirection, float deltaTime)
    {
        return Quaternion.Lerp(_stateMachine.transform.rotation, 
        Quaternion.LookRotation(moveDirection), 
        _stateMachine.RotationDamping * deltaTime);
    }
    
}
