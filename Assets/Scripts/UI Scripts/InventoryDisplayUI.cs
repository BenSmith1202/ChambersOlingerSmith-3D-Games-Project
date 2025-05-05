using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;

public class InventoryDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject itemDisplayPanel;
    [SerializeField] private GameObject itemIconPrefab;
    [SerializeField] private Transform itemIconContainer;

    [Header("Settings")]
    [SerializeField] private float spaceBetweenItems = 10f;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private BuffManager playerBuffManager;
    private Dictionary<string, ItemDisplay> activeItemDisplays = new Dictionary<string, ItemDisplay>();
    public CameraControllerScript cam;
    LogicManager logicManager;
    public GameObject pauseMenu; //FOR LOGIC MANAGER
    private void Start()
    {
        try
        {
            logicManager = GameObject.FindWithTag("LogicManager").GetComponent<LogicManager>();
        }
        catch(Exception e)
        {
            logicManager = null;
        }

        cam = GameObject.FindWithTag("MainCamera").GetComponent<CameraControllerScript>();
        itemDisplayPanel.GetComponent<HorizontalLayoutGroup>().spacing = spaceBetweenItems;
        // Find the player's BuffManager
        playerBuffManager = GameObject.FindWithTag("Player").GetComponent<BuffManager>();

        if (playerBuffManager == null)
        {
            Debug.LogError("InventoryDisplayUI: Could not find player BuffManager!");
        }

        // Initialize UI as hidden
        itemDisplayPanel.SetActive(false);
    }

    private void Update()
    {
        
        // Toggle display with TAB key
        if (logicManager != null && Input.GetKeyDown(toggleKey) && !logicManager.isTimeSlowed)
        { 
            
            ShowItemDisplay();
        }
        else if (logicManager != null && Input.GetKeyUp(toggleKey) && !logicManager.isTimeSlowed)
        {
            
            HideItemDisplay();
        }
    }

    public void ShowItemDisplay()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        itemDisplayPanel.SetActive(true);
        cam.GetComponent<CameraControllerScript>().camLock = true;
        UpdateItemDisplay();
    }

    public void HideItemDisplay()
    {
        ItemTooltipSystem.HideTooltip();
        
        //only return look control if the item window isnt currently locking the screen
        if (logicManager != null && !logicManager.isTimeSlowed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cam.GetComponent<CameraControllerScript>().camLock = false;
        }
        
        itemDisplayPanel.SetActive(false);
    }

    public void UpdateItemDisplay()
    {
        // First clear any outdated entries
        ClearInactiveItems();

        // Group items by type and count occurrences (stacks)
        Dictionary<string, int> itemCounts = new Dictionary<string, int>();
        Dictionary<string, ItemInstance> itemInstances = new Dictionary<string, ItemInstance>();

        foreach (var item in playerBuffManager.allItems)
        {
            string itemID = item.itemName;

            // Count this item
            if (itemCounts.ContainsKey(itemID))
            {
                itemCounts[itemID]++;
            }
            else
            {
                itemCounts[itemID] = 1;
                itemInstances[itemID] = item;
            }
        }

        // Now update or create UI elements for each item type
        foreach (var itemEntry in itemCounts)
        {
            string itemID = itemEntry.Key;
            int count = itemEntry.Value;
            ItemInstance itemInstance = itemInstances[itemID];

            if (activeItemDisplays.ContainsKey(itemID))
            {
                // Update existing display
                activeItemDisplays[itemID].UpdateCount(count);
            }
            else
            {
                // Create new display
                GameObject newItemIcon = Instantiate(itemIconPrefab, itemIconContainer);
                //get item display from child
                ItemDisplay itemDisplay = newItemIcon.GetComponentInChildren<ItemDisplay>();
                

                itemDisplay.Setup(itemInstance, count);
                activeItemDisplays.Add(itemID, itemDisplay);

                // Position it in the grid
                LayoutRebuilder.ForceRebuildLayoutImmediate(itemIconContainer as RectTransform);
            }
        }
    }

    private void ClearInactiveItems()
    {
        // Get all current item IDs
        HashSet<string> currentItems = new HashSet<string>();
        foreach (var item in playerBuffManager.allItems)
        {
            currentItems.Add(item.itemName);
        }

        // Find displays that no longer correspond to items in inventory
        List<string> itemsToRemove = new List<string>();
        foreach (var displayEntry in activeItemDisplays)
        {
            if (!currentItems.Contains(displayEntry.Key))
            {
                itemsToRemove.Add(displayEntry.Key);
                Destroy(displayEntry.Value.gameObject);
            }
        }

        // Remove them from the dictionary
        foreach (var itemID in itemsToRemove)
        {
            activeItemDisplays.Remove(itemID);
        }
    }
}