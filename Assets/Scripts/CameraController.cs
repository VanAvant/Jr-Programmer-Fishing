using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject focalPoint;
    private Vector3 cameraOffset;
    // Start is called before the first frame update

    private float cameraZoomSpeed = 250;
    private float cameraMinDistance = 10;
    private float cameraMaxDistance = 35;

    private float cameraOrbitSpeedY = 150;
    private float minCameraAngleY = 30f;
    private float maxCameraAngleY = 75f;

    void Start()
    {
        CentreCameraOnPlayer();
        //transform.LookAt(focalPoint.transform.position);
        UpdateCameraOffset();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ZoomCamera();
        OrbitCamera();
        transform.position = focalPoint.transform.position + cameraOffset;
    }

    private void UpdateCameraOffset()
    {
        cameraOffset = transform.position - focalPoint.transform.position;
    }

    private void ZoomCamera()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            float distanceFromPlayer = cameraOffset.magnitude;

            if (cameraOffset.magnitude > cameraMinDistance && Input.mouseScrollDelta.y > 0 || cameraOffset.magnitude < cameraMaxDistance && Input.mouseScrollDelta.y < 0)
            {
                cameraOffset += transform.forward * Time.deltaTime * cameraZoomSpeed * Input.mouseScrollDelta.y;
            }
        }
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



//private void OrbitCamera()
//{
//    if (Input.GetMouseButton(1))
//    {
//        //    mainCamera.transform.LookAt(transform.position, Vector3.up);

//        //    float currentAngle = GetCameraAngle();

//        //    Debug.Log("CurrentAngle " + currentAngle);

//        //    if (currentAngle < maxCameraAngleY && Input.GetAxis("Mouse Y") < 0f || currentAngle > minCameraAngleY && Input.GetAxis("Mouse Y") > 0f)
//        //    {
//        //        mainCamera.transform.RotateAround(transform.position, transform.forward, Input.GetAxis("Mouse Y") * Time.deltaTime * cameraOrbitSpeedX);
//        //    }

//        //    mainCamera.transform.RotateAround(transform.position, transform.up, Input.GetAxis("Mouse X") * Time.deltaTime * cameraOrbitSpeedX);
//        //    UpdateCameraOffset();

//        Quaternion rotateAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * Time.deltaTime * cameraOrbitSpeedX, Vector3.up);

//        cameraOffset = rotateAngle * cameraOffset;
//    }

//    mainCamera.transform.LookAt(focalPoint.transform.position);
//}

//private void OrbitCamera()
//{
//    if (Input.GetMouseButton(1))
//    {
//        Quaternion rotateAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * Time.deltaTime * cameraOrbitSpeedX, Vector3.up);

//        cameraOffset = rotateAngle * cameraOffset;
//    }
//    mainCamera.transform.LookAt(focalPoint.transform.position);
//}

//private float GetCameraAngle()
//{
//    Vector3 vectorToCamera = mainCamera.transform.position - focalPoint.transform.position;
//    // Convert to 2d I.e. product of X and Z vs Y 

//    float XZproduct = Mathf.Sqrt((vectorToCamera.x * vectorToCamera.x) + (vectorToCamera.z * vectorToCamera.z));

//    ////TEST CODE, proves XZproduction calculation is correct
//    //float XZproduct2 = Vector2.Distance(new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.z), new Vector2(transform.position.x, transform.position.z));
//    //Debug.Log("XY: " + XZproduct + ", XZ2: " + XZproduct2);

//    return 90 - Mathf.Rad2Deg * Mathf.Atan(XZproduct / vectorToCamera.y);
//}