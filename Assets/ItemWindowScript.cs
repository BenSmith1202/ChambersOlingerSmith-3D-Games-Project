using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWindowScript : MonoBehaviour
{

    List<ItemInstance> allItems = new List<ItemInstance>();
    [SerializeField] public List<ItemInstance> commonItems = new List<ItemInstance>();
    [SerializeField] public List<ItemInstance> rareItems = new List<ItemInstance>();
    [SerializeField] public List<ItemInstance> legendaryItems = new List<ItemInstance>();

    public GameObject itemCardPrefab;

    public Vector2 card1pos = new(-450, 0);
    public Vector2 card2pos = new(0, 0);
    public Vector2 card3pos = new(450, 0);

    public List<GameObject> itemCards = new List<GameObject>(); //should be 3 of these

    public GameObject tint;
    GameObject cam;
    GameObject player;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void OpenWindow(int rarity)
    {
        Time.timeScale = 0;
        //UnlockMouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        tint.SetActive(true);

        // PAUSE INPUTS (TODO: make a universal input pause)
        cam.GetComponent<CameraControllerScript>().camLock = true;
        player.GetComponent<PlayerControllerScript>().inputPaused = true;

        // Add up the union of the different rarities lists int allItems:
        allItems.AddRange(commonItems);
        allItems.AddRange(rareItems);
        allItems.AddRange(legendaryItems);

        DrawCards(rarity); // draw three common cards
    }

    public void CloseWindow()
    {
        Time.timeScale = 1;
        tint.SetActive(false);
        //LockMouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // UNPAUSE INPUTS (TODO: make a universal input pause)
        cam.GetComponent<CameraControllerScript>().camLock = false;
        player.GetComponent<PlayerControllerScript>().inputPaused = false;
        ClearCards();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWindow();
        }
    }

    public void DrawCards(int rarity)
    {
        //clear cards
        foreach (GameObject card in itemCards)
        {
            Destroy(card);
        }

        itemCards.Clear();
        List<ItemInstance> itemPool = new List<ItemInstance>();

        // Select item pool based on rarity
        switch (rarity)
            {
            case 0:
                itemPool = commonItems;
                break;
            case 1:
                itemPool = rareItems;
                break;
            case 2:
                itemPool = legendaryItems;
                break;
            default:
                Debug.LogError("Invalid rarity index");
                break;
        }

        // Create Card 1, Adding it to the list, picking an item for it, and setting its position
        itemCards.Add(Instantiate(itemCardPrefab, transform));
        itemCards[0].GetComponent<ItemCardScript>().SetItem(itemPool[Random.Range(0, itemPool.Count)]);
        itemCards[0].GetComponent<RectTransform>().anchoredPosition = card1pos;

        // Create Card 2
        itemCards.Add(Instantiate(itemCardPrefab, transform));
        itemCards[1].GetComponent<ItemCardScript>().SetItem(itemPool[Random.Range(0, itemPool.Count)]);
        itemCards[1].GetComponent<RectTransform>().anchoredPosition = card2pos;

        //Create Card 3
        itemCards.Add(Instantiate(itemCardPrefab, transform));
        itemCards[2].GetComponent<ItemCardScript>().SetItem(itemPool[Random.Range(0, itemPool.Count)]);
        itemCards[2].GetComponent<RectTransform>().anchoredPosition = card3pos;

    }

    public void ClearCards()
    {
        //Later, Animations may be added
        foreach (GameObject card in itemCards)
        {
            Destroy(card);
        }
        itemCards.Clear();
    }
}
