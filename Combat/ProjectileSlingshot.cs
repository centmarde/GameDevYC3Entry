using UnityEngine;

public class ProjectileSlingshot : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private LayerMask hitMask;

    private Rigidbody rb;
    private float damage;
    private object source;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // make sure collider is trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void Launch(Vector3 velocity, float dmg, object src)
    {
        damage = dmg;
        source = src;
        rb.linearVelocity = velocity;                 // <-- fix

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // only react to layers we care about
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;

        var target = other.GetComponentInParent<IDamageable>();
        if (target != null && target.IsAlive)
        {
            Vector3 hitPoint = transform.position;
            Vector3 hitNormal = -rb.linearVelocity.normalized;  // <-- fix
            target.TakeDamage(damage, hitPoint, hitNormal, source);
        }

        Destroy(gameObject);
    }
}
