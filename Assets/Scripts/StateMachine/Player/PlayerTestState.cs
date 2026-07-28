using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerTestState : PlayerBaseState
{
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private Transform _cameraTransform;
    private float _speed => _stateMachine._speed;
    private float _rotationSpeed => _stateMachine._rotationSpeed;
    public PlayerTestState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
       _stateMachine.Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }
    }
    public override void Tick(float deltaTime)
    {
        _moveInput = _stateMachine.Input.MovementValue;
         CalculateMovementDirection();
         Move();
         RotateTowardsMovement();
    }

    private void Move()
    {
            _stateMachine.Rigidbody.linearVelocity = new Vector3(_moveDirection.x * _speed, _stateMachine.Rigidbody.linearVelocity.y, _moveDirection.z * _speed);
    }

    private void RotateTowardsMovement()
    {
if (_moveDirection == Vector3.zero) return;

        if (_cameraTransform != null)
        {
            // Take the forward direction of the camera as the reference for rotation
            Vector3 camForward = _cameraTransform.forward;
            
            // Project the camera's forward vector onto the horizontal plane
            // This ensures that the character doesn't tilt up or down when the camera is looking up or down
            camForward.y = 0;

            if (camForward != Vector3.zero)
            {
                camForward.Normalize();
                
                // Calculate the target rotation based on the camera's forward direction
                Quaternion targetRotation = Quaternion.LookRotation(camForward);
                
                // Smoothly rotate the character towards the target rotation
                _stateMachine.Rigidbody.rotation = Quaternion.RotateTowards(_stateMachine.Rigidbody.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
            }
        }    }

    private void CalculateMovementDirection()
    {

        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        _moveDirection = (camForward * _moveInput.y + camRight * _moveInput.x);

        if (_moveDirection.magnitude > 1.0f)
        {
            _moveDirection.Normalize();
        }
    }

    public override void Exit()
    {
        
    }
    
}
