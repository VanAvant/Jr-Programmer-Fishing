using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera titleCamera;

    //Game variables 
    public bool gameIsRunning = false; 

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
    }
}
