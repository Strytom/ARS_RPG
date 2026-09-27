using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using System;

public class Targeter : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup cinemachineTargetGroup;
    private List<Target> targets = new List<Target>();
    private Camera mainCamera;
    public Target CurrentTarget{get; private set;}

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target))
        {return;}

            targets.Add(target);
            target.OnDestroyed += OnTargetDestroyed;
        
    }
    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target))
        {return;}

            OnTargetDestroyed(target);
        
    }

    private void OnTargetDestroyed(Target target)
    {
        if(CurrentTarget == target)
        {
            cinemachineTargetGroup.RemoveMember(CurrentTarget.transform);
            CurrentTarget = null;
        }

        target.OnDestroyed -= OnTargetDestroyed;
        targets.Remove(target);
    }

    public bool SelectTarget()
    {
        if (targets.Count == 0)
        { return false; }

        Target closestTarget = null;
        float closestTargetDistance = Mathf.Infinity;

        
    foreach (Target target in targets)
    {
        // 1. Получаем Vector3 (x, y — экранные координаты 0..1, z — глубина перед камерой)
        Vector3 viewPos = mainCamera.WorldToViewportPoint(target.transform.position);

        // 2. Если z <= 0, объект находится ПОЗАДИ камеры
        if (viewPos.z <= 0)
        {
            continue;
        }

        // 3. Проверяем, находится ли объект в границах экрана
        if (viewPos.x < 0f || viewPos.x > 1f || viewPos.y < 0f || viewPos.y > 1f)
        {
            continue;
        }

        // 4. Считаем расстояние от центра экрана (0.5, 0.5)
        Vector2 toCenter = new Vector2(viewPos.x - 0.5f, viewPos.y - 0.5f);
        if (toCenter.sqrMagnitude < closestTargetDistance)
        {
            closestTarget = target;
            closestTargetDistance = toCenter.sqrMagnitude;
        }
    }

        if (closestTarget == null) { return false; }

        CurrentTarget = closestTarget;

        cinemachineTargetGroup.AddMember(CurrentTarget.transform, 1f, 0f);

        return true;
    }
    private Target NearestTarget()
    {
        Target nearestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (Target target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTarget = target;
            }
        }
        return nearestTarget;
    }

    public void ClearTarget()
    {
        if (CurrentTarget == null) return;

        cinemachineTargetGroup.RemoveMember(CurrentTarget.transform);
        CurrentTarget = null;
       
    }
}
