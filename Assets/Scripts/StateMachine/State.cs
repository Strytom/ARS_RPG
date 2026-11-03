using UnityEngine;

public abstract class State
{
    public abstract void Enter();
    public abstract void Tick(float deltaTime);
    public abstract void Exit();
    protected float GetNormalizedTime(Animator animator,string tag)
    {
        AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextAnimatorStateInfo = animator.GetNextAnimatorStateInfo(0);

        if (animator.IsInTransition(0) &&  nextAnimatorStateInfo.IsTag(tag))
        {
            return nextAnimatorStateInfo.normalizedTime;
        }
        else if (! animator.IsInTransition(0) && currentAnimatorStateInfo.IsTag(tag))
        {
            return currentAnimatorStateInfo.normalizedTime;
        }
        else
        {
            return 0f;
        }
        
    }
}
