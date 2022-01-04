using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCShark : NPCFish
{
    GameManager gameManager;

    //private bool isChasingFish;
    private bool hasChosenFish;
    private GameObject targetFish;

    private float positionUpdateFrequency = 0.4f;
    private float persuitRange = 100;

    [SerializeField] Material red;

    //Navmesh speeds

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        navPoint.GetComponent<MeshRenderer>().material = red;

    }
    public override IEnumerator FishDefaultRoutine()
    {
        float timeSinceUpdate = 0;
        while (true)
        {
            if (timeSinceUpdate < positionUpdateFrequency)
            {
                timeSinceUpdate += Time.deltaTime;
            }

            else
            {
                timeSinceUpdate = 0;

                if (hasChosenFish && GetRangeToFish() < persuitRange)
                {
                    {
                        MoveToTarget(targetFish.transform.position, navPoint);
                    }
                }

                else
                {
                    targetFish = GetClosestFish();
                    hasChosenFish = true;
                }
            }
            yield return null;
        }

        float GetRangeToFish()
        {
            return Vector3.Distance(transform.position, targetFish.transform.position);
        }
    }

    public override void ResetFish()
    {
        hasChosenFish = false;
        //isChasingFish = false;
    }

    private GameObject GetClosestFish() //For loop over list, calculate distance then choose closest;
    {
        GameObject closestFish;
        float closestFishDistance;

        SetClosestFish(gameManager.fishList[0]);

        for (int i = 1; i < gameManager.fishList.Count; i++)
        {
            if (Vector3.Distance(transform.position, gameManager.fishList[i].transform.position) < closestFishDistance)
            {
                SetClosestFish(gameManager.fishList[i]);
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

