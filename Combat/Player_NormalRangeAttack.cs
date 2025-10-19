using UnityEngine;

public class Player_NormalRangeAttack : Player_RangeAttack
{

    [Header("Base Data")]
    [SerializeField] private Player_NormalRangeAtkData_SO baseData;

    private Player_NormalRuntimeData runtimeData;

    // ------------------------------------------
    protected override void Awake()
    {
        base.Awake();

        runtimeData = new Player_NormalRuntimeData(baseData);
    }

    // Called by WaveManager at the start of every run
    public void ResetRuntimeStats()
    {
        runtimeData = new Player_NormalRuntimeData(baseData);
    }

    //Called when the player receives a buff after a wave
    public void ApplyBuff(float dmgBonus, float spdBonus)
    {
        runtimeData.ApplyUpgrade(dmgBonus, spdBonus);
    }

    protected override void FireProjectile()
    {
        base.FireProjectile();
        Vector3 dir = cachedDirection.sqrMagnitude > DirectionEpsilon
           ? cachedDirection
           : muzzle.forward;

        Vector3 spawnPos = muzzle.position + dir.normalized * muzzleForwardOffset;
        spawnPos.y += muzzleHeightOffset;

        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        ProjectileSlingshot p = Instantiate(projectile, spawnPos, rot);
        p.transform.SetParent(null, true);

        var projCol = p.GetComponent<Collider>();
        if (projCol && player)
        {
            foreach (var c in player.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(projCol, c, true);
        }

        p.Launch(
                   dir.normalized * runtimeData.normalAttackSpeed,
                   runtimeData.normalAttackDamage,
                   this );
    }
}
