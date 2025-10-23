using UnityEngine;


[CreateAssetMenu(menuName = "MonsterBook/Entry")]
public class MonsterEntry : ScriptableObject
{
    public string entryName;
    [TextArea(3, 10)] public string description;
    public string location;
    public Sprite image;
    public string region;
    public bool discovered;
}
