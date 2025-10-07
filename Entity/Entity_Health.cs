using UnityEngine;

[DisallowMultipleComponent]
public class Entity_Health : MonoBehaviour, IDamageable
{
    private Entity entity;

    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool invulnerable = false;

    public bool IsAlive => currentHealth > 0f;

    public event System.Action<float, Vector3, Vector3, object> OnDamaged;
    public event System.Action OnDeath;

    private void Awake()
    {
        entity = GetComponent<Entity>();
    }

    private void OnEnable()
    {
        currentHealth = Mathf.Max(1f, maxHealth);   // or stats.GetMaxHealth()
    }

    public bool TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, object source)
    {
        if (!IsAlive || invulnerable) return false;

        float applied = Mathf.Max(0f, damage);
        if (applied <= 0f) return false;

        currentHealth = Mathf.Max(0f, currentHealth - applied);

        OnDamaged?.Invoke(damage, hitPoint, hitNormal, source);


        if (!IsAlive)
            Die();

        return true;
    }

    private void Die()
    {
        // Single path to death: let Entity coordinate what happens next
        OnDeath?.Invoke();
        entity?.EntityDeath();
        // Do NOT Destroy here; the concrete Entity (e.g., Enemy) decides timing.
    }

    // helpers if you need them
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth; // or stats.GetMaxHealth()
    public float HealthPercent => currentHealth / Mathf.Max(1f, MaxHealth);

    public void SetMaxHealth(float value)
    {
        maxHealth = Mathf.Max(1f, value);   // make sure it’s at least 1
        currentHealth = maxHealth;          // reset current to full
    }

}
