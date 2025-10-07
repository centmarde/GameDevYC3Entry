using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Item", fileName = "ItemData - ")]
public class Item_DataSO : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
}
