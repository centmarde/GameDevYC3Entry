using UnityEngine;


public interface IDamageable
{
    bool IsAlive { get; }
    bool TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, object source);
}