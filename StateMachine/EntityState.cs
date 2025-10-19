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
        Debug.Log($"[EntityState] Enter - Setting animator bool '{animBoolName}' to TRUE");
        if (anim == null)
        {
            Debug.LogError("[EntityState] Animator is NULL!");
        }
        else
        {
            anim.SetBool(animBoolName, true);
            Debug.Log($"[EntityState] Animator bool '{animBoolName}' set successfully");
        }
    }

    public virtual void Update()
    {

    }

    public virtual void Exit()
    {
        Debug.Log($"[EntityState] Exit - Setting animator bool '{animBoolName}' to FALSE");
        anim.SetBool(animBoolName, false);
    }


}
