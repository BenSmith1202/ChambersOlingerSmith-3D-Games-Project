using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardScript : MonoBehaviour
{
    GameObject player;
    BuffManager playerBuffManager;
    ItemWindowScript itemWindowScript;
    public ItemInstance representedItem;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text rarityText;
    public Image iconImage;

    public Color commonCardColor;
    public Color rareCardColor;
    public Color legendaryCardColor;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerBuffManager = player.GetComponent<BuffManager>();
        GameObject itemWindow = transform.parent.gameObject;
        itemWindowScript = itemWindow.GetComponent<ItemWindowScript>();
        SetItem(representedItem);
    }

  

    public void SetItem(ItemInstance representedItem)
    {
        if (representedItem == null) 
        { 
            Debug.LogWarning("No item to set :(");
            return;
        }
        this.representedItem = representedItem;
        nameText.SetText(representedItem.itemName);
        descriptionText.SetText(representedItem.description);
        rarityText.SetText(representedItem.rarity.ToString());
        iconImage.sprite = representedItem.icon;
        Image cardImage = GetComponent<Image>();
        switch (representedItem.rarityIndex)
        {
            case 0:
                cardImage.color = commonCardColor;
                break;
            case 1:
                cardImage.color = rareCardColor;
                break;
            case 2:
                cardImage.color = legendaryCardColor;
                break;
            default:
                Debug.LogError("Invalid rarity index");
                break;
        }
    }
    public void PickItem()
    {
        GetComponent<Button>().interactable = false; // no double picksies
        playerBuffManager.AddItem(representedItem); //give the player the item
        itemWindowScript.CloseWindow(); //close the item window
        //clear cards and close the item window
    }
}
