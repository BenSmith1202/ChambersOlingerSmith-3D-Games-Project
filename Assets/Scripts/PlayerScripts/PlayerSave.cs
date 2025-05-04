using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSave
{
    public string filename;
    public List<int> itemIDList;
    public int level;
    public PlayerSave(string filename, List<int> idList, int level)
    {
        this.filename = filename;
        this.itemIDList = idList;
        this.level = level;
    }

    public void SaveThisFile()
    {
        // Implement the logic to save this file
        Debug.Log("Saving file: " + filename);

        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + filename, JsonUtility.ToJson(this));

    }

    public static PlayerSave LoadThisFile(string name)
    {
        // Implement the logic to load this file
        Debug.Log("Loading file: " + name);
        string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + name);
        Debug.Log("Loaded save file at " + Application.persistentDataPath + "/" + name);
        PlayerSave loadedFile = JsonUtility.FromJson<PlayerSave>(json);
        return loadedFile;
    }
}