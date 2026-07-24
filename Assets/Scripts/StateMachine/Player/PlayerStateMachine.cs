using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    [field: SerializeField] public InputReader Input { get; private set; }
    private void Start() {
        SwitchState(new PlayerTestState(this));
    }
}
