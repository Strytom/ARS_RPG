using UnityEngine;
using UnityEngine.AI;

public class Ragdoll : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody _mainRigidbody, _ledgeDetectorRigidbody;
    [SerializeField] private Collider _mainCollider, _ledgeDetector;
    [SerializeField] private NavMeshAgent _navMeshAgent; // Опционально (для врагов)

    private Collider[] _ragdollColliders;
    private Rigidbody[] _ragdollRigidbodies;

    private void Awake()
    {
        // 1. Автоматический поиск ссылок, если они не заданы в инспекторе
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_mainRigidbody == null) _mainRigidbody = GetComponent<Rigidbody>();
        if (_mainCollider == null) _mainCollider = GetComponent<Collider>();
        //if (_navMeshAgent == null) _navMeshAgent = GetComponent<NavMeshAgent>();

        // 2. Находим все дочерние коллайдеры и риджидбоди (в костях персонажа)
        _ragdollColliders = GetComponentsInChildren<Collider>(true);
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);

        // 3. На старте рэгдолл выключен (живой персонаж)
        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool isRagdoll)
    {
        // Управляем костями скелета (исключая главный корневой коллайдер)
        foreach (Collider col in _ragdollColliders)
        {
            if (col == _mainCollider || col == _ledgeDetector) continue;
            if (col.CompareTag("WeaponLogic")) continue; // Игнорируем коллайдер оружия
            col.enabled = isRagdoll;
        }

        // Управляем физикой костей скелета (исключая главный корневой Rigidbody)
        foreach (Rigidbody rb in _ragdollRigidbodies)
        {
            if (rb == _mainRigidbody || rb == _ledgeDetectorRigidbody) continue;
            if (rb.CompareTag("WeaponLogic")) continue; // Игнорируем Rigidbody оружия
            rb.isKinematic = !isRagdoll;
            rb.useGravity = isRagdoll;
        }

        // Отключаем/включаем управление живым персонажем
        if (_mainCollider != null) _mainCollider.enabled = !isRagdoll;
        if (_mainRigidbody != null) _mainRigidbody.isKinematic = isRagdoll;
        if (_animator != null) _animator.enabled = !isRagdoll;
        if (_navMeshAgent != null) _navMeshAgent.enabled = !isRagdoll;
    }
}