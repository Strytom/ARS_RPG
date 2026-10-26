using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{
    public EnemyDeadState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        _stateMachine.Ragdoll.ToggleRagdoll(true);
        _stateMachine.Weapon.gameObject.SetActive(false);
        GameObject.Destroy(_stateMachine.Target);
    }

    public override void Exit()
    {
    }

    public override void Tick(float deltaTime)
    {
    }
}
