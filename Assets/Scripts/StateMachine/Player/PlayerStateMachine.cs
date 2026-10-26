using UnityEngine;
using Unity.Cinemachine;

public class PlayerStateMachine : StateMachine
{
    [Header("State Machine References")]
    [field: SerializeField] public InputReader Input { get; private set; }
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public Targeter Targeter { get; private set; }
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public CinemachineCamera cineFreeLookCamera { get; private set; }
    [field: SerializeField] public CinemachineCamera cineTargetCamera { get; private set; }


    [Header("Player References")]
    [field: SerializeField] public float FreeLookSpeed { get; private set; } = 5f;
    [field: SerializeField] public float RotationDamping { get; private set; } = 2.5f;
    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }
    [field: SerializeField] public Attack[] Attack { get; private set; }
    [field: SerializeField] public WeaponDamage Weapon { get; private set; }
    [field: SerializeField] public CrossHairTarget Crosshair { get; private set; }
    

    public Transform CameraTransform { get; private set; }
    

    private void Start() {
        CameraTransform = Camera.main.transform;

        SwitchState(new PlayerFreeLookState(this));
    }
    private void OnEnable()
    {
        Health.OnDamageTaken += HandleDamageTaken;
    }
    private void OnDisable()
    {
        Health.OnDamageTaken -= HandleDamageTaken;
    }

    private void HandleDamageTaken()
    {
        SwitchState(new PlayerImpactState(this));
    }
}
