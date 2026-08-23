using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using System;

public class Targeter : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup cinemachineTargetGroup;
    private List<Target> targets = new List<Target>();
    public Target CurrentTarget{get; private set;}

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target))
        {return;}

            targets.Add(target);
            target.OnDestroyed += OnTargetDestroyed;
        
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

    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target))
        {return;}

            OnTargetDestroyed(target);
        
    }
    public bool SelectTarget()
    {
        if (targets.Count == 0)
        { return false; }

        CurrentTarget = NearestTarget();
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
