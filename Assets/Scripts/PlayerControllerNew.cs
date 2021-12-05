using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerControllerNew : MonoBehaviour
{
    private NavMeshAgent playerAgent;



    //Movement & animation variables
    [SerializeField] MeshCollider islandCollider;
    private float playerSpeed = 8;
    private float playerAcceleration = 120;
    private float playerAngularSpeed = 360;

    public float minDistanceFromCast = 2;
    [SerializeField] Animator playerAnim;

    //NavMesh variables
    [SerializeField] GameObject navPointPrefab;
    private GameObject navPoint;

    //camera variables
    [SerializeField] Camera mainCamera;
    Vector3 cameraOffset;
    private float cameraZoomSpeed = 250;
    private float cameraMinDistance = 10;
    private float cameraMaxDistance = 35;

    private float cameraOrbitSpeedX = 150;
    private float minCameraAngleX = 30;
    private float maxCameraAngleX = 75;


    // Start is called before the first frame update
    void Start()
    {
        playerAgent = GetComponent<NavMeshAgent>();
        playerAgent.speed = playerSpeed;
        playerAgent.acceleration = playerAcceleration;
        playerAgent.angularSpeed = playerAngularSpeed;

        UpdateCameraOffset();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            HandlePrimaryMouseClick();
        }

        ZoomCamera();
        OrbitCamera();
    }


    private void LateUpdate()
    {
        mainCamera.transform.position = transform.position + cameraOffset;    
    }

    private void ZoomCamera()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            float distanceFromPlayer = cameraOffset.magnitude;

            if(cameraOffset.magnitude > cameraMinDistance && Input.mouseScrollDelta.y > 0 || cameraOffset.magnitude < cameraMaxDistance && Input.mouseScrollDelta.y < 0)
            {
                cameraOffset += mainCamera.transform.forward * Time.deltaTime * cameraZoomSpeed * Input.mouseScrollDelta.y;
            }
        }
    }

    private void OrbitCamera()
    {
        if (Input.GetMouseButton(1))
        {
            Quaternion cameraRotation = mainCamera.transform.rotation;

            Debug.Log("CameraRotation.x: " + cameraRotation.x);

            if (cameraRotation.x >= minCameraAngleX && cameraRotation.x <= maxCameraAngleX) //Doesn't work due to quarternions
            {
               
                mainCamera.transform.RotateAround(transform.position, Vector3.forward, Input.GetAxis("Mouse Y") * Time.deltaTime * cameraOrbitSpeedX);
                UpdateCameraOffset();
            }

            //mainCamera.transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * Time.deltaTime * cameraOrbitSpeedX);
            
        }

    }

  
    private void HandlePrimaryMouseClick()
    {
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(cursorRay, out hit)) // if ray hits a collider (should always be true)
        {
            if (hit.collider.gameObject.name == "Island") //If it hits the island, move to point
            {
                NavMeshHit discard;
                if (NavMesh.SamplePosition(hit.point, out discard, 0.5f, 1))
                MovePlayerToTarget(hit.point);
            }

            else
            {
                if (hit.collider.gameObject.name == "Sea") //Else, if it hits the sea: (This would not usually be required but it allows the use of colliders to 'block off' zones from being clicked)
                {
                    NavMeshHit closestPoint;

                    if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(0)) //If player moves cursor over sea while holding down left mouse, move to closest edge 
                    {
                        NavMesh.FindClosestEdge(hit.point, out closestPoint, 1);
                        MovePlayerToTarget(closestPoint.position);
                    }
                    else
                    {
                        if (GetDistanceToAxis(hit.point) > minDistanceFromCast) // If the player is further from the clicked point that the minimum cast distance, move toward that point
                        {
                            NavMesh.SamplePosition(hit.point, out closestPoint, 100f, 1);

                            Vector3 minDistanceFromHit = (hit.point - transform.position).normalized * minDistanceFromCast;

                            Vector3 pointMinDistanceFromHit = GetIslandHeightAtPosition(hit.point - minDistanceFromHit);

                            if (Vector3.Distance(transform.position, closestPoint.position) < Vector3.Distance(transform.position, pointMinDistanceFromHit)) //If player is closer to edge, move there, otherwise move to minDistanceFromCast units away and cast.
                            {
                                MovePlayerToTarget(closestPoint.position);
                            }
                            else
                            {
                                MovePlayerToTarget(pointMinDistanceFromHit, true);
                            }
                        }
                        else //If the player is within minimum casting range, start fishing
                        {
                            StartCoroutine(FishingRoutine(hit.point));
                        }
                    }
                }
            }
        }
    }

    private Vector3 GetIslandHeightAtPosition(Vector3 point)
    {
        Ray ray = new Ray(new Vector3(point.x, 100, point.z), Vector3.down);
        RaycastHit raycastHit;

        islandCollider.Raycast(ray, out raycastHit, 200f);

        return raycastHit.point;

    }//Returns the position of the top of the island geometry at a passed point 

    private void MovePlayerToTarget(Vector3 target, bool startFishing = false) //Uses NavMesh to move player to passed point
    {

        playerAgent.SetDestination(target);
        playerAnim.SetBool("b_IsPlayerMoving", true);

        Object.Destroy(navPoint);
        navPoint = Instantiate(navPointPrefab, target, navPointPrefab.transform.rotation);
        if (startFishing)
        {
            navPoint.GetComponent<NavPoint>().CastOnContact = true;
        }
    } 

    private float GetDistanceToAxis(Vector3 target)
    {
        return (new Vector2(target.x, target.z) - new Vector2(transform.position.x, transform.position.z)).magnitude;
    }//Returns distance from player to passed point's y-axis

    IEnumerator FishingRoutine(Vector3 target)
    {
        playerAgent.ResetPath();
        Debug.Log("Casting");
        MovePlayerToTarget(transform.position);

        Vector3 lookAtPoint = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(lookAtPoint);

        playerAnim.Play("Villager@Fishing01");

        bool cancelFishing = false; 

        while (!cancelFishing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                cancelFishing = true;
            }
            yield return null;
        }
        Debug.Log("Finished fishing");
        yield break;
    }

    private void UpdateCameraOffset()
    {
        cameraOffset = mainCamera.transform.position - transform.position;
    }
    private void OnTriggerEnter(Collider other) //detect nav points (for animation)
    {
        //Debug.Log("Trigger detected");
        if (other.gameObject.CompareTag("NavPoint"))
        {
            if (other.GetComponent<NavPoint>().CastOnContact)
            {
                StartCoroutine(FishingRoutine(other.transform.position));
            }
            Object.Destroy(other.gameObject);
            playerAnim.SetBool("b_IsPlayerMoving", false);
        }
    }

}
