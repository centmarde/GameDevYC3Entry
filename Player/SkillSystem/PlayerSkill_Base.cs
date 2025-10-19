using System;
using UnityEngine;

public class PlayerSkill_Base : MonoBehaviour
{
    [SerializeField] private PlayerSkill_DataSO skillData;
    public PlayerSkill_DataSO Data => skillData;
    public Player player { get; private set; }
    public PlayerSkill_Manager skillManager { get; private set; }

    private float lastTimeUsed;

    protected virtual void Awake()
    {
        skillManager = GetComponentInParent<PlayerSkill_Manager>();
        player = GetComponentInParent<Player>();
        lastTimeUsed = lastTimeUsed - Data.cooldown;

    }
    public virtual bool CanUseSkill()
    {
        if (OnCoolDown())
        {
            return false;
        }

        return true;
    }

    protected bool OnCoolDown()
    {
        if (Data == null) return false; // No cooldown if no data
        return Time.time < lastTimeUsed + Data.cooldown;
    }
    public void SetSkillOnCoolDown() => lastTimeUsed = Time.time;

}
