using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltipSystem : MonoBehaviour
{
    [SerializeField] private GameObject tooltipContainer;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private float offsetX = 20f;
    [SerializeField] private float offsetY = 20f;

    private static ItemTooltipSystem instance;
    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the tooltip system exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        rectTransform = tooltipContainer.GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        // Hide tooltip at start
        HideTooltip();
    }

    private void Update()
    {
        if (tooltipContainer.activeSelf)
        {
            // Follow mouse position with offset
            Vector2 mousePosition = Input.mousePosition;
            Vector2 adjustedPosition = new Vector2(mousePosition.x + offsetX, mousePosition.y + offsetY);

            // Keep tooltip on screen
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Calculate boundaries to keep tooltip on screen
                float width = rectTransform.rect.width;
                float height = rectTransform.rect.height;

                if (adjustedPosition.x + width > Screen.width)
                    adjustedPosition.x = mousePosition.x - width - offsetX;

                if (adjustedPosition.y + height > Screen.height)
                    adjustedPosition.y = mousePosition.y - height - offsetY;

                rectTransform.position = adjustedPosition;
            }
            else
            {
                // For other render modes, may need to convert to canvas space
                rectTransform.position = adjustedPosition;
            }
        }
    }

    public static void ShowTooltip(string title, string description, Rarity rarity)
    {
        if (instance == null) return;

        instance.titleText.text = title;
        instance.descriptionText.text = description;

        // Set rarity text and color
        instance.rarityText.text = rarity.rarityName;
        instance.rarityText.color = rarity.rarityColor;

        instance.tooltipContainer.SetActive(true);
    }

    public static void HideTooltip()
    {
        if (instance == null) return;
        instance.tooltipContainer.SetActive(false);
    }
}