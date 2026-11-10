using UnityEngine;

public class PlayerClimbingState : PlayerBaseState
{
    private readonly int _climbingHash = Animator.StringToHash("Climbing");

    private readonly Vector3 _ledgePoint;
    private readonly Vector3 _ledgeForward;

    private const float CrossFadeDuration = 0.1f;
    private const float SurfaceGap = 0.02f;

    private CapsuleCollider _capsule;
    private Vector3 _startPosition;
    private Vector3 _endPosition;

    private bool _oldGravity;
    private bool _oldKinematic;
    private bool _oldColliderEnabled;
    private bool _prepared;

    public PlayerClimbingState( PlayerStateMachine stateMachine, Vector3 ledgePoint, Vector3 ledgeForward) : base(stateMachine)
    {
        _ledgePoint = ledgePoint;
        _ledgeForward = ledgeForward;
    }

    public override void Enter()
    {
        Rigidbody rb = _stateMachine.Rigidbody;
        _capsule = rb.GetComponent<CapsuleCollider>();

        Vector3 forward = Vector3.ProjectOnPlane(_ledgeForward, Vector3.up);

        if (_capsule == null || forward.sqrMagnitude < 0.001f)
            return;

        forward.Normalize();

        // Ищем поверхность немного глубже края.
        Vector3 probePosition = _ledgePoint + forward * _stateMachine.ClimbForwardDistance + Vector3.up * 2f;

        if (!Physics.Raycast(probePosition, Vector3.down, out RaycastHit hit, 4f, _stateMachine.ClimbSurfaceMask, QueryTriggerInteraction.Ignore))
        {
            Debug.LogWarning("Climbing: верхняя поверхность не найдена.");
            return;
        }

        // Для этого варианта нужна относительно ровная площадка.
        if (Vector3.Angle(hit.normal, Vector3.up) > 30f)
        {
            Debug.LogWarning("Climbing: поверхность слишком крутая.");
            return;
        }

        _startPosition = rb.position;

        // Учитываем реальное расположение нижней точки капсулы.
        float rootToBottom = rb.position.y - _capsule.bounds.min.y;

        _endPosition = new Vector3(hit.point.x, hit.point.y + rootToBottom + SurfaceGap, hit.point.z);

        _oldGravity = rb.useGravity;
        _oldKinematic = rb.isKinematic;
        _oldColliderEnabled = _capsule.enabled;

        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        _capsule.enabled = false;

        _prepared = true;

        _stateMachine.Animator.CrossFadeInFixedTime(_climbingHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        if (!_prepared)
        {
            _stateMachine.SwitchState(new PlayerFallState(_stateMachine));
            return;
        }

        Animator animator = _stateMachine.Animator;
        AnimatorStateInfo info;

        // Во время CrossFade читаем новую анимацию.
        if (animator.IsInTransition(0))
        {
            info = animator.GetNextAnimatorStateInfo(0);
        }
        else
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
        }

        if (info.shortNameHash != _climbingHash)
            return;

        float progress = Mathf.Clamp01(info.normalizedTime);

        // Сначала поднимаем тело, затем переносим на площадку.
        Vector3 raisedPosition = new Vector3( _startPosition.x, _endPosition.y, _startPosition.z);

        Vector3 position;

        if (progress < 0.6f)
        {
            float t = Mathf.SmoothStep(0f, 1f, progress / 0.6f);
            position = Vector3.Lerp(_startPosition, raisedPosition, t);
        }
        else
        {
            float t = Mathf.SmoothStep(0f, 1f, (progress - 0.6f) / 0.4f);

            position = Vector3.Lerp(raisedPosition, _endPosition, t);
        }

        // Управляемое позиционирование кинематического тела.
        _stateMachine.Rigidbody.position = position;

        if (info.normalizedTime >= 1f)
        {
            _stateMachine.Rigidbody.position = _endPosition;
            ReturnToLocomotion();
            return;
        }
    }

    public override void Exit()
    {
        if (!_prepared)
            return;

        Rigidbody rb = _stateMachine.Rigidbody;

        _capsule.enabled = _oldColliderEnabled;
        rb.isKinematic = _oldKinematic;
        rb.useGravity = _oldGravity;
    }
}