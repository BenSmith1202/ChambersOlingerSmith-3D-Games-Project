using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI stackCountText;
    [SerializeField] private Image rarityBorder;

    private ItemInstance itemInstance;

    public void Setup(ItemInstance item, int count)
    {
        itemInstance = item;

        // Set icon
        iconImage.sprite = item.icon;

        // Set rarity border color if applicable
        if (rarityBorder != null)
        {
            rarityBorder.sprite = item.icon;
            rarityBorder.color = item.rarity.rarityColor;
        }

        // Set stack count
        UpdateCount(count);
    }

    public void UpdateCount(int count)
    {
        stackCountText.text = count > 1 ? count.ToString() : "";
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipSystem.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemInstance != null)
        {
            ItemTooltipSystem.ShowTooltip(itemInstance.itemName, itemInstance.description, itemInstance.rarity);
        }
    }
}