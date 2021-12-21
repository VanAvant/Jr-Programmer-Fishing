using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameManager gameManager;

    [SerializeField] GameObject focalPoint;
    private Vector3 cameraOffset;
    // Start is called before the first frame update

    private float cameraZoomSpeed = 250;
    private float cameraMinDistance = 10;
    private float cameraMaxDistance = 35;

    private float cameraOrbitSpeedY = 150;

    void Start()
    {
        CentreCameraOnPlayer();
        UpdateCameraOffset();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (gameManager.gameIsRunning)
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                ZoomCamera();
            }

            OrbitCamera();
            
            CentreCameraOnPlayer();
        }
    }

    private void UpdateCameraOffset()
    {
        cameraOffset = transform.position - focalPoint.transform.position;
    }

    public virtual void ZoomCamera()
    {

        cameraOffset += transform.forward * Time.deltaTime * cameraZoomSpeed * Input.mouseScrollDelta.y;

        if (cameraOffset.magnitude > cameraMaxDistance)
        {
            cameraOffset = cameraOffset.normalized * cameraMaxDistance;
        }
        else
        {
            if (cameraOffset.magnitude < cameraMinDistance)
            {
                cameraOffset = cameraOffset.normalized * cameraMinDistance;
            }
        }

        transform.position = focalPoint.transform.position + cameraOffset; //Camera zoom
    }

    public virtual void ZoomCamera(float multiplier)
    {
        cameraOffset = cameraOffset.normalized * multiplier;
        ZoomCamera(); //Calling zoomCamera here effectively sets the camera to max zoom
    }


    private void OrbitCamera()
    {
        if (Input.GetMouseButton(1))
        {
            transform.RotateAround(focalPoint.transform.position, Vector3.up, Time.deltaTime * cameraOrbitSpeedY * Input.GetAxis("Mouse X"));
            UpdateCameraOffset();
        }
    }//Needs work

    private void CentreCameraOnPlayer()
    {
        //Draw plane at camera
        Plane plane = new Plane(-Vector3.up, transform.position.y); //Vector.up is negative because the ray origin is below the plane. Not sure why it matters but it does 
        float distance;

        //Draw ray from player to plane along camera facing 

        Ray ray = new Ray(focalPoint.transform.position, -transform.forward);


        //Get hit point move camera to that position

        plane.Raycast(ray, out distance);

        transform.position = ray.GetPoint(distance);
    }
}