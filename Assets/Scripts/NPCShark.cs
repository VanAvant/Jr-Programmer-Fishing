using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCShark : NPCFish
{
    GameManager gameManager;

    private bool isChasingFish;
    private bool hasChosenFish;
    private GameObject targetFish;
    private List<GameObject> fishList;

    //Navmesh speeds

    // Start is called before the first frame update

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        fishList = gameManager.fishList; //ERROR IS SOMEWHERE HERE


    }
    public override IEnumerator FishDefaultRoutine()
    {
        while (true)
        {
            if (!hasChosenFish)
            {
                targetFish = GetClosestFish();
            }

            if (!isChasingFish)
            {
                MoveToTarget(targetFish.transform.position, navPoint);
            }
            yield return null;
        }
    }

    private GameObject GetClosestFish() //For loop over list, calculate distance then choose closest;
    {
        GameObject closestFish;
        float closestFishDistance;

        Debug.Log("FishList 0: " + fishList[0].name); //List is not instantiating properly
        SetClosestFish(fishList[0]);

        for (int i = 1; i > fishList.Count; i++)
        {
            if (Vector3.Distance(transform.position, fishList[i].transform.position) < closestFishDistance)
            {
                SetClosestFish(fishList[i]);
            }
        }

        return closestFish;

        void SetClosestFish(GameObject fish)
        {
            closestFish = fish;
            closestFishDistance = Vector3.Distance(transform.position, fish.transform.position);
        }
    }


}

