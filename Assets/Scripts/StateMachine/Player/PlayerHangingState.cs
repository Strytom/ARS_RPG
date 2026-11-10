using UnityEngine;

public class PlayerHangingState : PlayerBaseState
{
    private readonly int HangingHash = Animator.StringToHash("Hanging");
    private const float CrossFadeDuration = 0.1f;
    private Vector3 closestPoint;
    private Vector3 ledgeForward;

    public PlayerHangingState(PlayerStateMachine stateMachine, Vector3 ledgePosition, Vector3 ledgeForward) : base(stateMachine)
    {
        this.closestPoint = ledgePosition;
        this.ledgeForward = ledgeForward;
    }

    public override void Enter()
    {
        _stateMachine.transform.rotation = Quaternion.LookRotation(ledgeForward, Vector3.up);
        _stateMachine.transform.position = closestPoint - (_stateMachine.LedgeDetector.transform.position - _stateMachine.transform.position); 
        _stateMachine.Animator.CrossFadeInFixedTime(HangingHash, CrossFadeDuration);
        _stateMachine.Rigidbody.useGravity = false;
        _stateMachine.Rigidbody.linearVelocity = Vector3.zero;
        Debug.Log("Player is now hanging from the ledge.");
    }

    public override void Tick(float deltaTime)
    {
        if (_stateMachine.Input.MovementValue.y > 0)
        {
           _stateMachine.SwitchState(new PlayerClimbingState(_stateMachine, closestPoint, ledgeForward));
            Debug.Log("Player is climbing up from the ledge.");
            return;
        }
        else if (_stateMachine.Input.MovementValue.y < 0)
        {
            _stateMachine.SwitchState(new PlayerFallState(_stateMachine));
            Debug.Log("Player released the ledge and is now falling.");
            return;
        }
    }

    public override void Exit()
    {
        _stateMachine.Rigidbody.useGravity = true;
    }
}
