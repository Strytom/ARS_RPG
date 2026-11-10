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
    [field: SerializeField] public Ragdoll Ragdoll { get; private set; }
    [field: SerializeField] public LedgeDetector LedgeDetector { get; private set; }
    [field: SerializeField] public GroundChecker GroundChecker { get; private set; }
    [field: SerializeField] public WeaponDamage Weapon { get; private set; }
    [field: SerializeField] public CrossHairTarget Crosshair { get; private set; }
    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }
    [field: SerializeField] public Transform CameraTransform { get; private set; }
    [field: SerializeField] public CinemachineCamera cineFreeLookCamera { get; private set; }
    [field: SerializeField] public CinemachineCamera cineTargetCamera { get; private set; }

    public bool IsGrounded => GroundChecker != null && GroundChecker.IsGrounded;

    [Header("Player References")]
    public float PreviousDodgeTime { get; private set; } = Mathf.NegativeInfinity;
    [field: SerializeField] public float DodgeDuration { get; private set; }
    [field: SerializeField] public float DodgeLength { get; private set; }
    [field: SerializeField] public float DodgeCooldown { get; private set; }
    [field: SerializeField] public float JumpForce { get; private set; }
    [field: SerializeField] public float FreeLookSpeed { get; private set; } = 5f;
    [field: SerializeField] public float RotationDamping { get; private set; } = 2.5f;
    [field: SerializeField] public Attack[] Attack { get; private set; }
    [field: SerializeField] public LayerMask ClimbSurfaceMask { get; private set; }
    [field: SerializeField] public float ClimbForwardDistance { get; private set; } = 0.5f;
    

    private void Start() {
        CameraTransform = Camera.main.transform;

        SwitchState(new PlayerFreeLookState(this));
    }
    private void OnEnable()
    {
        Health.OnDamageTaken += HandleDamageTaken;
        Health.OnDeath += HandleDeath;
    }
    private void OnDisable()
    {
        Health.OnDamageTaken -= HandleDamageTaken;
        Health.OnDeath -= HandleDeath;
    }

    private void HandleDamageTaken()
    {
        SwitchState(new PlayerImpactState(this));
    }

    private void HandleDeath()
    {
        SwitchState(new PlayerDeadState(this));
    }
    public void SetDodgeTime(float dodgeTime)
    {
        PreviousDodgeTime = dodgeTime;
    }

}
