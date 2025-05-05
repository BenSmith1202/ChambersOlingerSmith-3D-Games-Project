using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSavingScript : MonoBehaviour
{

    BuffManager buffManager;
    EntityStats playerStats;
    [SerializeField] ItemDirectory itemDirectory;
    List<int> itemIDList;
    int level;
    // Start is called before the first frame update
    void Start()
    {
        itemIDList = new List<int>();
        buffManager = GetComponent<BuffManager>();
        playerStats = GetComponent<EntityStats>();
        int level = playerStats.level;

        // load player save if the file exists
        if (System.IO.File.Exists(Application.persistentDataPath + "/playerSave.json"))
        {
            LoadPlayer();
        }
        else
        {
            Debug.Log("No save file found, starting fresh.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    buffManager.AddItem(itemDirectory.allItems[Random.Range(0, itemDirectory.allItems.Count)]);
        //}
        //if (Input.GetKeyDown(KeyCode.X))
        //{
        //    SavePlayerToFile();
        //}
        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    LoadPlayer();
        //}
    }

    public void SavePlayerToFile()
    {
        // Implement the logic to save targets to a file
        Debug.Log("Saving Player to file...");

        //get all targets

        foreach (var item in buffManager.allItems)
        {
            itemIDList.Add(itemDirectory.GetItemID(item));
        }

        int level = playerStats.level;

        
        // Create a SaveFile object
        PlayerSave saveFile = new PlayerSave("playerSave.json", itemIDList, level);
        saveFile.SaveThisFile();
    }

    public void LoadPlayer()
    {
        if (!System.IO.File.Exists(Application.persistentDataPath + "/playerSave.json"))
        {
            Debug.Log("No save file found.");
        }
        //clearing
        if (playerStats != null)
            playerStats.level = 0; // Set level to 0 if no save file exists
        return;
        List<ItemInstance> toRemove = new List<ItemInstance>();
        foreach (var item in buffManager.allItems)
        {
            toRemove.Add(item);
        }
        foreach (var item in toRemove)
        {
            buffManager.RemoveItem(item);
        }

        //Loading
        PlayerSave save = PlayerSave.LoadThisFile("playerSave.json");

        //load items in loop
        foreach (var itemID in save.itemIDList)
        {
            ItemInstance item = itemDirectory.GetItem(itemID);
            buffManager.AddItem(item);
        }

        //level up
        playerStats.LevelUp(save.level); //level up to the level in the save file

    }
}
