using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    [Header("State Machine References")]
    [field: SerializeField] public InputReader Input { get; private set; }
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
    [Header("Player References")]
    [field: SerializeField] public float _speed { get; private set; } = 5f;
    [field: SerializeField] public float _rotationSpeed { get; private set; } = 720f;

    private void Start() {
        SwitchState(new PlayerTestState(this));
    }
}
