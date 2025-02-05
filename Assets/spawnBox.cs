using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnBox : MonoBehaviour
{


    public GameObject boxPrefab;

    List<Vector3> positions = new List<Vector3>();

    public static class GameEvent
    {
        public const string boxDeath = "DOOR_KEY_1";
    }



    // Start is called before the first frame update
    void Start()
    {
        positions.Add(new Vector3(4, 7, 20));
        positions.Add(new Vector3(15, 3, 4));
        positions.Add(new Vector3(27, 10, 13));
        positions.Add(new Vector3(19, 4, 0));
        positions.Add(new Vector3(0, 7, 16));
    }



    void OnEnable()
    {
        Messenger.AddListener(GameEvent.boxDeath, OnDeath);
    }


    void OnDisable()
    {
        Messenger.RemoveListener(GameEvent.boxDeath , OnDeath);
    }



    void OnDeath()
    {
        GameObject box = Instantiate(boxPrefab, positions[ Random.Range(0, 5)], Quaternion.identity);
    }



    // Update is called once per frame
    void Update()
    {

    }
}
