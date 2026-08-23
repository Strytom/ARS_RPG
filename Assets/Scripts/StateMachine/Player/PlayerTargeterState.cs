using System;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerTargeterState : PlayerBaseState
{
    public PlayerTargeterState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        _stateMachine.Input.OnTargetEvent += OnTarget;

        _stateMachine.Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        _stateMachine.cineFreeLookCamera.Priority = 0;
        _stateMachine.cineTargetCamera.Priority = 1;
    }

    public override void Tick(float deltaTime)
    {
        if (_stateMachine.Targeter.CurrentTarget == null)
        {
            _stateMachine.SwitchState(new PlayerFreeLookState(_stateMachine));
            return;
        }
        Move(deltaTime);
        FaceMovementTarget(deltaTime);
    }

    public override void Exit()
    {
        _stateMachine.Input.OnTargetEvent -= OnTarget;
    }

    private void Move(float deltaTime)
    {
        Vector3 _moveDirection = CalculateMovementDirection();

        _stateMachine.Rigidbody.linearVelocity = new Vector3(_moveDirection.x * _stateMachine.FreeLookSpeed, _stateMachine.Rigidbody.linearVelocity.y, _moveDirection.z * _stateMachine.FreeLookSpeed);

        if (_moveDirection == Vector3.zero)
        {
            _stateMachine.Rigidbody.linearVelocity = new Vector3(0f, _stateMachine.Rigidbody.linearVelocity.y, 0f);//Stop the player from moving when there is no input
            return;
        }
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
    private void FaceMovementTarget(float deltaTime)
    {
        if (_stateMachine.Targeter.CurrentTarget == null) return;

        Vector3 directionToTarget = _stateMachine.Targeter.CurrentTarget.transform.position - _stateMachine.transform.position;
        directionToTarget.y = 0; 

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        _stateMachine.Rigidbody.MoveRotation(Quaternion.Slerp(
            _stateMachine.Rigidbody.rotation, 
            targetRotation, 
            _stateMachine.RotationDamping * deltaTime));
        }
       
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

    private void OnTarget()
    {
        
        AlignFreeLookCameraWithCurrentView();
        _stateMachine.Targeter.ClearTarget();
        _stateMachine.SwitchState(new PlayerFreeLookState(_stateMachine)); 
    
    }
}
