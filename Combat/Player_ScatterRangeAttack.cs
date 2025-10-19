using UnityEngine;

public class Player_ScatterRangeAttack : Player_RangeAttack
{
    [Header("Scatter Shot Settings")]
    [SerializeField] private Player_ScatterRangeAtkData_SO baseData;

    private Player_ScatterRuntimeData runtimeData;

    protected override void Awake()
    {
        base.Awake();
        runtimeData = new Player_ScatterRuntimeData(baseData);
    }

    public void ResetRuntimeStats()
    {
        runtimeData = new Player_ScatterRuntimeData(baseData);
    }

    public void ApplyBuff(float dmgBonus, float speedBonus)
    {
        runtimeData.ApplyUpgrade(dmgBonus, speedBonus);
    }

    protected override void FireProjectile()
    {
        if (projectile == null || muzzle == null) return;

        for (int i = 0; i < 5; i++)
        {
            // Slight random Y-axis rotation to simulate spread
            Quaternion spreadRot = Quaternion.Euler(0f, Random.Range(-runtimeData.spreadAngle, runtimeData.spreadAngle), 0f);
            Vector3 dir = spreadRot * cachedDirection;

            Vector3 spawnPos = muzzle.position + dir.normalized * 0.25f;
            spawnPos.y -= 0.3f;

            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            ProjectileSlingshot p = Instantiate(projectile, spawnPos, rot);
            p.transform.SetParent(null, true);

            var projCol = p.GetComponent<Collider>();
            if (projCol && player)
            {
                foreach (var c in player.GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(projCol, c, true);
            }

            p.Launch(dir.normalized * runtimeData.pelletSpeed, runtimeData.pelletDamage, this);
        }
    }
}
