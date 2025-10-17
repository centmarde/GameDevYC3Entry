using UnityEngine;


[CreateAssetMenu(menuName = "Dagitab/Skills/Player Skill Data", fileName ="SkillData - ")]
public class PlayerSkill_DataSO : ScriptableObject 

{
    public string skillName;
    public string skillDescription;
    public float cooldown;
    public string skillType;

    [Header("Circling Projectiles Settings (if applicable)")]
    public int defaultProjectileCount = 2;
    public float projectileDamage = 2f;
    public float orbitRadius = 2f;
    public float orbitSpeed = 90f;

}
