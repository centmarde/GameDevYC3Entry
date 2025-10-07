using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public abstract class Entity : MonoBehaviour 
{
    public Animator anim { get; private set; }
    public Rigidbody rb { get; private set; }

    protected StateMachine stateMachine;


    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        stateMachine = new StateMachine();
        rb.useGravity = false;


    }


    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        stateMachine.UpdateActiveState();
    }


    public virtual void EntityDeath()
    {
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        
    }




}
