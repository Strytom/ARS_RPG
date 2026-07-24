using Unity.VisualScripting;
using UnityEngine;

public class PlayerTestState : PlayerBaseState
{
    private float _timer= 0;
    public PlayerTestState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        Debug.Log("Entered PlayerTestState");
        _stateMachine.Input.OnJumpEvent += HandleJump;
        _stateMachine.Input.OnDodgeEvent += HandleDodge;
    }
    public override void Tick(float deltaTime)
    {
        _timer += deltaTime;
            Debug.Log("Ticking: " + _timer);
    }
    public override void Exit()
    {
        _stateMachine.Input.OnJumpEvent -= HandleJump;
        _stateMachine.Input.OnDodgeEvent -= HandleDodge;
        Debug.Log("Exiting PlayerTestState");
    }
    private void HandleJump()
    {
        _stateMachine.SwitchState(new PlayerTestState(_stateMachine));
    }
    private void HandleDodge()
    {
        Debug.Log("Dodge event received in PlayerTestState");   
    }
}
