using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class ItemDirectory : MonoBehaviour
{
    [SerializeField] public List<ItemInstance> allItems = new List<ItemInstance>();

    public ItemInstance GetItem(int id)
    {
        // This method will be used to get an item from the directory.
        // It will take an item ID as a parameter and return the corresponding item instance.
        // You can implement this method based on your requirements.

        return allItems[id];
    }

    public int GetItemID(ItemInstance item)
    {
        // This method will be used to get the ID of an item.
        // It will take an item instance as a parameter and return the corresponding item ID.
        // checks for first item in the overall list that matches the name of the given item

        for (int i = 0; i < allItems.Count; i++)
        {
            if (allItems[i].itemName == item.itemName)
            {

                return i;
            }
        }
      
        Debug.Log("Item not found in the directory.");
        return -1;
    }
}
