using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerControllerNew : NavAgentScript
{
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
    [SerializeField] GameObject focalPoint;

    Vector3 cameraOffset;

    private float cameraZoomSpeed = 250;
    private float cameraMinDistance = 10;
    private float cameraMaxDistance = 35;

    private float cameraOrbitSpeedX = 150;
    private float minCameraAngleY = 30f;
    private float maxCameraAngleY = 75f;


    // Start is called before the first frame update
    void Start()
    {

        navAgent = GetComponent<NavMeshAgent>();

        navAgent.speed = playerSpeed;
        navAgent.acceleration = playerAcceleration;
        navAgent.angularSpeed = playerAngularSpeed;

        navPoint = Instantiate(navPointPrefab, navPointPrefab.transform.position, navPointPrefab.transform.rotation);

        UpdateCameraOffset();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            HandlePrimaryMouseClick();
        }

    }


    private void LateUpdate()
    {
        ZoomCamera();
        //OrbitCamera();
        mainCamera.transform.position = focalPoint.transform.position + cameraOffset;
    }

    private void UpdateCameraOffset()
    {
        cameraOffset = mainCamera.transform.position - focalPoint.transform.position;
    }

    private void ZoomCamera()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            float distanceFromPlayer = cameraOffset.magnitude;

            if (cameraOffset.magnitude > cameraMinDistance && Input.mouseScrollDelta.y > 0 || cameraOffset.magnitude < cameraMaxDistance && Input.mouseScrollDelta.y < 0)
            {
                cameraOffset += mainCamera.transform.forward * Time.deltaTime * cameraZoomSpeed * Input.mouseScrollDelta.y;
            }

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

    private void orbitCamera()
    {
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
        }
    }

    private float GetCameraAngle()
    {
        Vector3 vectorToCamera = mainCamera.transform.position - focalPoint.transform.position;
        // Convert to 2d I.e. product of X and Z vs Y 

        float XZproduct = Mathf.Sqrt((vectorToCamera.x * vectorToCamera.x) + (vectorToCamera.z * vectorToCamera.z));

        ////TEST CODE, proves XZproduction calculation is correct
        //float XZproduct2 = Vector2.Distance(new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.z), new Vector2(transform.position.x, transform.position.z));
        //Debug.Log("XY: " + XZproduct + ", XZ2: " + XZproduct2);

        return 90 - Mathf.Rad2Deg * Mathf.Atan(XZproduct / vectorToCamera.y);
    }

    private void HandlePrimaryMouseClick()
    {
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(cursorRay, out hit)) // if ray hits a collider (should always be true)
        {
            playerAnim.SetBool("b_IsPlayerMoving", true);

            if (hit.collider.gameObject.name == "Island") //If it hits the island, move to point
            {
                NavMeshHit discard;
                if (NavMesh.SamplePosition(hit.point, out discard, 0.5f, 1))
                    MoveToTarget(hit.point, navPoint);
            }

            else
            {
                if (hit.collider.gameObject.name == "SeaQuad") //Else, if it hits the sea: (This would not usually be required but it allows the use of colliders to 'block off' zones from being clicked)
                {
                    NavMeshHit closestPoint;

                    if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(0)) //If player moves cursor over sea while holding down left mouse, move to closest edge 
                    {
                        NavMesh.FindClosestEdge(hit.point, out closestPoint, 1);
                        MoveToTarget(closestPoint.position, navPoint); 
                    }
                    else
                    {
                        if (GetDistanceToAxis(hit.point) > minDistanceFromCast) // If the player is further from the clicked point that the minimum cast distance, move toward that point
                        {
                            NavMesh.SamplePosition(hit.point, out closestPoint, 100f, 1);

                            Vector3 minDistanceFromHit = (hit.point - transform.position).normalized * minDistanceFromCast;
                            Vector3 pointMinDistanceFromHit = GetRaycastAtPosition(hit.point - minDistanceFromHit).point;

                            if (Vector3.Distance(transform.position, closestPoint.position) < Vector3.Distance(transform.position, pointMinDistanceFromHit)) //If player is closer to edge, move there, otherwise move to minDistanceFromCast units away and cast.
                            {
                                MoveToTarget(closestPoint.position, navPoint);
                            }
                            else
                            {
                                MoveToTarget(pointMinDistanceFromHit, navPoint);
                                navPoint.GetComponent<NavPoint>().CastOnContact = true;
                            }
                        }
                        else //If the player is within minimum casting range, start fishing
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                StartCoroutine(FishingRoutine(hit.point));
                            }
                        }
                    }
                }
            }
        }
    }

    private float GetDistanceToAxis(Vector3 target)
    {
        return (new Vector2(target.x, target.z) - new Vector2(transform.position.x, transform.position.z)).magnitude;
    }//Returns distance from player to passed point's y-axis

    IEnumerator FishingRoutine(Vector3 target)
    {
        navAgent.ResetPath();
        Debug.Log("Casting");
        MoveToTarget(transform.position, navPoint);

        Vector3 lookAtPoint = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(lookAtPoint);

        playerAnim.Play("Villager@Fishing01");

        //Test code for logic purposes below this point

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


    private void OnTriggerEnter(Collider other) //detect nav points (for animation)
    {
        if (other.gameObject.CompareTag("NavPoint"))
        {
            if (other.GetComponent<NavPoint>().CastOnContact)
            {
                StartCoroutine(FishingRoutine(other.transform.position));
            }
            other.GetComponent<NavPoint>().CastOnContact = false;
            playerAnim.SetBool("b_IsPlayerMoving", false);
        }
    }

}
