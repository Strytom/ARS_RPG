using UnityEngine;

public class PlayerDeadState : PlayerBaseState
{
    public PlayerDeadState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        _stateMachine.Ragdoll.ToggleRagdoll(true);
        _stateMachine.Weapon.gameObject.SetActive(false);
    }

    public override void Exit()
    {
    }

    public override void Tick(float deltaTime)
    {
    }
}
