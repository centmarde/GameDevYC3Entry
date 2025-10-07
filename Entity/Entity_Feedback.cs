using UnityEngine;

public abstract class Entity_Feedback : MonoBehaviour
{
    protected Animator anim;
    protected Entity_Health health;
    protected Rigidbody rb;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        health = GetComponent<Entity_Health>();
        rb = GetComponent<Rigidbody>();

        if (health != null)
        {
            health.OnDamaged += OnDamaged;
            health.OnDeath += OnDeath;
        }
    }

    protected abstract void OnDamaged(float damage, Vector3 hitPoint, Vector3 hitNormal, object source);

    protected abstract void OnDeath();



}
