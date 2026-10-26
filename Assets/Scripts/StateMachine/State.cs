using UnityEngine;

public abstract class State
{
    public abstract void Enter();
    public abstract void Tick(float deltaTime);
    public abstract void Exit();
    protected float GetNormalizedTime(Animator animator)
    {
        AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextAnimatorStateInfo = animator.GetNextAnimatorStateInfo(0);

        if (animator.IsInTransition(0) &&  nextAnimatorStateInfo.IsTag("Attack"))
        {
            return nextAnimatorStateInfo.normalizedTime;
        }
        else if (! animator.IsInTransition(0) && currentAnimatorStateInfo.IsTag("Attack"))
        {
            return currentAnimatorStateInfo.normalizedTime;
        }
        else
        {
            return 0f;
        }
        
    }
}
