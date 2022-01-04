using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Camera variables
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera titleCamera;

    //Game variables 
    public bool gameIsRunning = false;

    [SerializeField] GameObject sharkPrefab;
    protected GameObject shark;
    [SerializeField] List<GameObject> fishPrefabList;
    public List<GameObject> fishList;

    private int qtyActiveFish = 20;

    void Awake()
    {
        fishList = new List<GameObject>();
    }

    public void StartGame()
    {
        
        if (!gameIsRunning)
        {
            Debug.Log("Game has started");
            gameIsRunning = true;
            mainCamera.GetComponent<CameraController>().ZoomCamera(100);
            titleCamera.enabled = false;

            SpawnInitialFish();
            shark = Instantiate(sharkPrefab, sharkPrefab.GetComponent<NPCShark>().GetRandomPosition(), sharkPrefab.transform.rotation);
        }
    }

    private void SpawnInitialFish()
    {
        //Populate fishPrefab list with fish

        for (int i = 0; i < qtyActiveFish; i++)
        {
            //Get spawn position
            //If spawn position is on camera, mirror X and Z 

            int nextFishType = Random.Range(0, fishPrefabList.Count);

            //Debug.Log("Next fish type: " + nextFishType);

            GameObject newFish = Instantiate(fishPrefabList[nextFishType], fishPrefabList[nextFishType].transform.position, fishPrefabList[nextFishType].transform.rotation); //Rotation doesn't matter as it is controlled by navmeshagent;

            if (IsPointOnScreen(newFish.transform.position))
            {
                //Debug.Log("Mirroring position " + newFish.transform);

                Vector3 newPos = newFish.transform.position;

                newPos.x = -newPos.x;
                newPos.y = -newPos.y;

                newFish.transform.position = newPos;

                //Debug.Log("New position " + newFish.transform);
            }

            fishList.Add(newFish);
        }
    }

    public bool IsPointOnScreen(Vector3 point) //Not sure if this is working
    {
        Vector3 pointOnScreen = mainCamera.WorldToViewportPoint(point);

        //Debug.Log("Point on screen: " + pointOnScreen);

        if (pointOnScreen.z > 0)
        {
            if (IsInBounds(pointOnScreen.x))
            {
                if (IsInBounds(pointOnScreen.y))
                {
                    return true;
                }
            }
        }

        return false;

        bool IsInBounds( float axis)  
        {
            if(axis > 0 && axis < 1)
            {
                return true;
            }
            return false;
        }   
    }
}
