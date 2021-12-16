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
    [SerializeField] List<GameObject> fishPrefabList;
    private List<GameObject> fishList;

    private int qtyActiveFish = 15;


    // Start is called before the first frame update
    void Start()
    {
        //mainCamera.transform.SetPositionAndRotation(titleCamera.transform.position, titleCamera.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        gameIsRunning = true;
        mainCamera.GetComponent<CameraController>().ZoomCamera(100);
        titleCamera.enabled = false;

        SpawnInitialFish();
    }

    private void SpawnInitialFish()
    {
        //Populate fishPrefab list with fish
        //Spawn shark

        for (int i = 0; i < qtyActiveFish; i++)
        {
            //Get spawn position
            //If spawn position is on camera, mirror X and Z 

            int nextFishType = Random.Range(0, fishPrefabList.Count);
            Debug.Log("Next fish type: " + nextFishType);

            Vector3 spawnPosition = fishPrefabList[nextFishType].GetComponent<NPCFish>().GetRandomPosition(); //Something is wrong here, always returns exactly the same values
            Debug.Log("Random position: " + spawnPosition);


            if (IsPointOnScreen(spawnPosition))
            {
                Debug.Log("Mirrored position");
                spawnPosition.x = -spawnPosition.x;
                spawnPosition.y = -spawnPosition.y;
            }

            GameObject newFish = Instantiate(fishPrefabList[nextFishType], spawnPosition, transform.rotation); //Rotation doesn't matter as it is controlled by navmeshagent;

            //fishList.Add(newFish);
        }
    }

    private bool IsPointOnScreen(Vector3 point)
    {
        Vector3 pointOnScreen = mainCamera.WorldToViewportPoint(point);

        Debug.Log("Point on screen: " + pointOnScreen);

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


//TO DO:
//Manage qty of fish 
//Including 1 shark that patrols & scatters fish in range

//Spawning fish function (Get position, if in view of camera, mirror/rotate
