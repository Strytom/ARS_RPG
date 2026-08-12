using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    [Header("State Machine References")]
    [field: SerializeField] public InputReader Input { get; private set; }
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [Header("Player References")]
    [field: SerializeField] public float FreeLookSpeed { get; private set; } = 5f;
    [field: SerializeField] public float RotationDamping { get; private set; } = 10f;

    public Transform CameraTransform { get; private set; }

    private void Start() {
        CameraTransform = Camera.main.transform;

        SwitchState(new PlayerFreeLookState(this));
    }
}
