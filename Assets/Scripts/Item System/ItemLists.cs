using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLists
{ 
    [SerializeField] public List<ItemInstance> allItems = new List<ItemInstance>();
    [SerializeField] public List<ItemInstance> commonItems = new List<ItemInstance>();
    [SerializeField] public List<ItemInstance> rareItems = new List<ItemInstance>();
    [SerializeField] public List<ItemInstance> legendaryItems = new List<ItemInstance>();
}
