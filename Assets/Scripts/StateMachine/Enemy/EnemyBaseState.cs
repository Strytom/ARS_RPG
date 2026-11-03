using UnityEngine;

public abstract class EnemyBaseState : State
{
    protected EnemyStateMachine _stateMachine;

    public EnemyBaseState(EnemyStateMachine stateMachine)
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
            moveDirection.x * _stateMachine.Speed,
             _stateMachine.Rigidbody.linearVelocity.y, 
             moveDirection.z * _stateMachine.Speed)+ _stateMachine.ForceReceiver.Move;
        _stateMachine.Rigidbody.linearVelocity = newVelocity;
    }
    protected void FaceMovementTarget(float deltaTime)
    {
        if (_stateMachine.Player == null) return;

        Vector3 directionToTarget = _stateMachine.Player.transform.position - _stateMachine.transform.position;
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
    protected void FaceTarget()
    {
        if (_stateMachine.Player == null) return;

        Vector3 directionToTarget = _stateMachine.Player.transform.position - _stateMachine.transform.position;
        directionToTarget.y = 0; 

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            _stateMachine.Rigidbody.MoveRotation(targetRotation);
        }
    }

    protected bool IsInChaseRange()
    {
        if (_stateMachine.Player.IsDead) { return false; }
        float distanceToPlayer = Vector3.Distance(_stateMachine.transform.position, _stateMachine.Player.transform.position);
        return distanceToPlayer <= _stateMachine.ChaseRange;
    }
}
