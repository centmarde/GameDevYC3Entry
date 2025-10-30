using UnityEngine;

[DisallowMultipleComponent]
public class Entity_Health : MonoBehaviour, IDamageable
{
    private Entity entity;

    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool invulnerable = false;

    private Player_Invulnerability invuln;

    // Evasion system
    private System.Func<bool> evasionCheck;

    public bool IsAlive => currentHealth > 0f;

    public event System.Action<float, Vector3, Vector3, object> OnDamaged;
    public event System.Action<float, Vector3, Vector3, object> OnEvaded;
    public event System.Action OnDeath;
    public event System.Action<float> OnHealed;


    private void Awake()
    {
        entity = GetComponent<Entity>();
        invuln = GetComponent<Player_Invulnerability>(); 

    }

    private void OnEnable()
    {
        currentHealth = Mathf.Max(1f, maxHealth);   // or stats.GetMaxHealth()
    }

    public bool TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, object source)
    {
        if (!IsAlive || invulnerable) return false;
        if (invuln != null && invuln.ShouldIgnoreDamage()) return false;

        float applied = Mathf.Max(0f, damage);
        if (applied <= 0f) return false;
        
        // Check for evasion
        if (evasionCheck != null && evasionCheck.Invoke())
        {
            OnEvaded?.Invoke(damage, hitPoint, hitNormal, source);
            Debug.Log($"{gameObject.name} evaded {applied} damage!");
            
            // Show evasion indicator (similar to critical hit indicator)
            EvasionFeedback.ShowEvasion(hitPoint);
            
            return false; // Damage was evaded
        }
        
        // Check for Defense skill absorption (for players only)
        PlayerSkill_Defense defenseSkill = GetComponent<PlayerSkill_Defense>();
        if (defenseSkill != null && defenseSkill.IsObtained)
        {
            float absorbed = defenseSkill.ProcessIncomingDamage(applied, hitPoint, source);
            applied -= absorbed; // Reduce damage by absorbed amount
            
            if (applied <= 0f)
            {
                // All damage was absorbed!
                return false;
            }
        }

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

        var playerComp = GetComponent<Player>();
        if (playerComp != null)
        {
            // Player death flow handled by Player_DeathState
            playerComp.RequestStateChange(playerComp.deathState);
            return; // ⬅️ DO NOT call EntityDeath() here — wait for DeathState to do it
        }
        entity?.EntityDeath();
        // Do NOT Destroy here; the concrete Entity (e.g., Enemy) decides timing.
    }

    // helpers if you need them
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth; // or stats.GetMaxHealth()
    public float HealthPercent => currentHealth / Mathf.Max(1f, MaxHealth);

    public void SetMaxHealth(float value)
    {
        maxHealth = Mathf.Max(1f, value);   // make sure it's at least 1
        currentHealth = maxHealth;          // reset current to full
    }

    /// <summary>
    /// Heal the entity by a specific amount (won't exceed max health)
    /// </summary>
    public void Heal(float amount)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealed?.Invoke(amount); // ✅ Notify listeners (like HUD)
    }
    /// <summary>
    /// Increase max health and optionally heal
    /// </summary>
    public void IncreaseMaxHealth(float amount, bool healToFull = false, bool preserveRatio = true)
    {
        //  Store previous ratio before changing max
        float healthPercent = currentHealth / Mathf.Max(1f, maxHealth);

        maxHealth += amount;

        if (healToFull)
        {
            // Option 1: heal completely
            currentHealth = maxHealth;
        }
        else if (preserveRatio)
        {
            // Option 2: keep same percentage (e.g., 100/100 → 150/150)
            currentHealth = maxHealth * healthPercent;
        }
        else
        {
            // Default behavior: keep raw current HP (can look visually “smaller”)
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        //  Notify HUD or listeners to refresh (like your HP bar)
        OnHealed?.Invoke(0);
    }

    /// <summary>
    /// Set the evasion check function (usually from Player stats)
    /// </summary>
    public void SetEvasionCheck(System.Func<bool> evasionFunction)
    {
        evasionCheck = evasionFunction;
    }

}
