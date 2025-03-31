using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCard : MonoBehaviour
{



    public float weight; // Relative chance to be selected


    public float creditCost; // Cost to spawn this monster


    public int minStage; // Earliest stage this monster can spawn

    public bool isFlyer;


    public float innerSpawnRadius;
    public float outerSpawnRadius;


}
