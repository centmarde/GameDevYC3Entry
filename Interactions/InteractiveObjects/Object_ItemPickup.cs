using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class Object_ItemPickup : MonoBehaviour , IInteractable
{
    [SerializeField] private Item_DataSO itemData;
    [SerializeField] private int itemAmount = 1;

    private InteractionProfile profile;


    private void OnValidate()
    {

        if(itemData == null) return;


        gameObject.name = "Object_ItemPickup - " + itemData.itemName;
    }

    public InteractionProfile GetProfile() => profile;


    public void Interact(Player player)
    {
       if(itemData == null)
        {
            Debug.Log("tried to interact but item data is missing", this);
            return;
        }

        Debug.Log($"Picked up {itemAmount} x {itemData.itemName}", this);

        Destroy(gameObject);

        //var inventory = player.GetComponent<Player_Inventory>();

        //if(inventory == null)
        // {
        //     Debug.Log("tried to interact but player has no inventory", this);
        //     return;
        // }

        // bool added = inventory.AddItem(itemData, itemAmount);

        // if(added)
        // {
        //    Debug.Log($"Picked up {itemAmount} x {itemData.itemName}", this);
        // } else
        // {
        //     Debug.Log($"Iventory full! Couldn't pickup {itemData.itemName}")
        // }
    }
}
