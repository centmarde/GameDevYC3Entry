using UnityEngine;

public abstract class EntityState
{

    protected StateMachine stateMachine;
    protected string animBoolName;
    protected Animator anim;
    protected Rigidbody rb;


    public EntityState(StateMachine stateMachine, string animBoolName)
    {
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;

    }

    public virtual void Enter()
    {
        if (anim == null)
        {
            return;
        }
        
        anim.SetBool(animBoolName, true);
    }

    public virtual void Update()
    {

    }

    public virtual void Exit()
    {
        anim.SetBool(animBoolName, false);
    }


}
