using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Enemy_AttackAnimEvents : MonoBehaviour
{
    [SerializeField] private Enemy_MeleeAttack melee;  // optional manual assign

    private void Awake()
    {
        if (!melee) melee = GetComponentInParent<Enemy_MeleeAttack>();
    }

    // must match the function names you set on the animation clip
    public void AnimEvent_DealDamage() { melee?.AnimEvent_DealDamage(); }
    public void AnimEvent_AttackEnd() { melee?.AnimEvent_AttackEnd(); }
}
