using UnityEngine;

public abstract class PlayerBaseState : State
{
    protected PlayerStateMachine _stateMachine;
    


    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this._stateMachine = stateMachine;
    }
    protected void Move(float deltaTime)
    {
        Move(Vector2.zero, deltaTime);
    }
    protected void Move(Vector3 moveDirection, float deltaTime)
    {
        Vector3 newVelocity = new Vector3(
            moveDirection.x * _stateMachine.FreeLookSpeed,
             _stateMachine.Rigidbody.linearVelocity.y, 
             moveDirection.z * _stateMachine.FreeLookSpeed)+ _stateMachine.ForceReceiver.Move * deltaTime;
        _stateMachine.Rigidbody.linearVelocity = newVelocity;
    }
    protected void FaceMovementTarget(float deltaTime)
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
    protected Vector3 CalculateMovementDirection(float deltaTime)
    {

        Vector3 camForward = _stateMachine.CameraTransform.forward;
        Vector3 camRight = _stateMachine.CameraTransform.right;
        Vector3 moveDirection = Vector3.zero;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();
        
        moveDirection = camRight * _stateMachine.Input.MovementValue.x + camForward * _stateMachine.Input.MovementValue.y;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);
        

        return moveDirection;
    }
    protected void ReturnToLocomotion()
    {
       if(_stateMachine.Targeter.CurrentTarget != null)
            {
                _stateMachine.SwitchState(new PlayerTargeterState(_stateMachine));
            }
            else
            {
                _stateMachine.SwitchState(new PlayerFreeLookState(_stateMachine));
            }
    }
}