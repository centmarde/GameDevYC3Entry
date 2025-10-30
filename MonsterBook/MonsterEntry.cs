using UnityEngine;


[CreateAssetMenu(menuName = "MonsterBook/Entry")]
public class MonsterEntry : ScriptableObject
{
    public string entryName;
    public string classification;
    public string appearance;
    public string origin;
    public string behavior;
    [TextArea(3, 10)] public string lore;
    public Sprite image;
    public bool discovered;
}
