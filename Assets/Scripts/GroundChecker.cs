using System;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class GroundChecker : MonoBehaviour
{
    [SerializeField] private CapsuleCollider _capsule;
    [SerializeField] private LayerMask _groundMask;

    [SerializeField, Min(0.01f)]
    private float _groundDistance = 0.08f;

    [SerializeField, Range(0f, 80f)]
    private float _maxGroundAngle = 55f;

    private const float CastStartOffset = 0.1f;

    public bool IsGrounded
    {
        get
        {
            if (_capsule == null ||
                !_capsule.enabled ||
                !_capsule.gameObject.activeInHierarchy)
                return false;

            GetCastGeometry(out Vector3 origin, out float radius);

            bool hitGround = Physics.SphereCast(
                origin,
                radius,
                Vector3.down,
                out RaycastHit hit,
                CastStartOffset + _groundDistance,
                _groundMask,
                QueryTriggerInteraction.Ignore);

            if (!hitGround)
                return false;

            // Стены и слишком крутые склоны не считаются землёй.
            return Vector3.Angle(hit.normal, Vector3.up)
                <= _maxGroundAngle;
        }
    }

    private void Awake()
    {
        if (_capsule == null)
            _capsule = GetComponent<CapsuleCollider>();
    }

    private void GetCastGeometry(
        out Vector3 origin, out float radius)
    {
        // Подходит для вашей вертикальной капсулы
        // с заблокированными наклонами Rigidbody.
        Bounds bounds = _capsule.bounds;

        radius = Mathf.Max(
            0.01f,
            Mathf.Min(bounds.extents.x, bounds.extents.z) * 0.9f);

        origin = new Vector3(
            bounds.center.x,
            bounds.min.y + radius + CastStartOffset,
            bounds.center.z);
    }
}
