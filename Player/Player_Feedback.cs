using UnityEngine;

public class Player_Feedback : Entity_Feedback
{
    private Player player;

    [SerializeField] private float knockbackForce = 30f;
    [SerializeField] private float knockbackDuration = 0.1f;
    private bool isKnockedBack = false;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
        
        // Subscribe to evasion events for feedback
        if (health != null)
        {
            health.OnEvaded += OnEvaded;
        }
    }
    
    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnEvaded -= OnEvaded;
        }
    }
    
    private void OnEvaded(float damage, Vector3 hitPoint, Vector3 hitNormal, object source)
    {
        // Optional: Play evasion animation
        // anim.SetTrigger("evade");
        
        // Optional: Play evasion sound
        // AudioManager.Instance?.PlaySound("Evade");
    }

    protected override void OnDamaged(float damage, Vector3 hitPoint, Vector3 hitNormal, object source)
    {
        anim.SetTrigger("hurt");

        if (rb != null && !isKnockedBack)
            StartCoroutine(ApplyKnockback(hitNormal));
    }

    private System.Collections.IEnumerator ApplyKnockback(Vector3 hitNormal)
    {
        isKnockedBack = true;

        Vector3 knockDir = -new Vector3(hitNormal.x, 0f, hitNormal.z).normalized;
        rb.AddForce(knockDir * knockbackForce, ForceMode.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isKnockedBack = false;
    }

    protected override void OnDeath()
    {


    }
}
