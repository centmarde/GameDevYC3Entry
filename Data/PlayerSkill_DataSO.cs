using UnityEngine;


[CreateAssetMenu(menuName = "Dagitab/Skills/Player Skill Data", fileName ="SkillData - ")]
public class PlayerSkill_DataSO : ScriptableObject 

{
    public string skillName;
    public string skillDescription;
    public float cooldown;
    public string skillType;

}
