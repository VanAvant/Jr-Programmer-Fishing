using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerControllerNew : NavAgentScript
{
    [SerializeField] GameManager gameManager;

    //Movement & animation variables
    [SerializeField] MeshCollider islandCollider;
    private float playerSpeed = 8;
    private float playerAcceleration = 120;
    private float playerAngularSpeed = 360;

    private float minCastDistanceFromTarget = 5.0f;
    private float maxCastDistanceFromTarget = 25.0f;
    [SerializeField] Animator playerAnim;

    private float mouseClickDuration = .1f;
    private float mouseDragDelta = 0.0f;

    //NavMesh variables
    [SerializeField] GameObject navPointPrefab;
    private GameObject navPoint;
    private int navPointLayer = 6;

    private string islandName = "Island";
    private int islandMeshArea = 1;
    private string seaName = "SeaQuad";
    private int seaMeshArea = 3;

    //Fishing variables 
    [SerializeField] GameObject floatPrefab;
    private float castSpeed = 15.0f;
    private Vector3 fishingTarget;


    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        navAgent.speed = playerSpeed;
        navAgent.acceleration = playerAcceleration;
        navAgent.angularSpeed = playerAngularSpeed;

        navPoint = Instantiate(navPointPrefab, navPointPrefab.transform.position, navPointPrefab.transform.rotation);

        playerAnim.Play("Villager@Fishing02");

        Physics.gravity *= 10;
    }

    void Update()
    {
        if (gameManager.gameIsRunning)
        {
            if (Input.GetMouseButton(0))
            {
                HandlePrimaryClick1();
            }
            UpdateMouseDelta();
        }
    }

    private RaycastHit GetClickTarget()
    {
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Physics.Raycast(cursorRay, out hit, Mathf.Infinity);

        return hit;

    }

    private void MouseDragMove(RaycastHit cursorRaycast)
    {
        //RaycastHit cursorRaycast = GetClickTarget();

        NavMeshHit discard;

        if (NavMesh.SamplePosition(cursorRaycast.point, out discard, 30f, islandMeshArea)) //Contained within if statement because navmesh is rough in places due to premade island asset and causes unpredicatable behaviour otherwise
        {
            MoveToTarget(discard.position, navPoint);

            if (Vector3.Distance(transform.position, discard.position) > 1f)
            {
                playerAnim.SetBool("b_IsPlayerMoving", true);
            }
        }
    }

    private void UpdateMouseDelta()
    {
        if (Input.GetMouseButton(0))
        {
            mouseDragDelta += Time.deltaTime;
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            //if (mouseDragDelta >= mouseClickDuration) //For debugging logic only
            //{
            //    Debug.Log("Was drag");
            //}
            //else
            //{
            //    Debug.Log("Was click");
            //}
            mouseDragDelta = 0;
        }
    }

    private void HandlePrimaryClick1()
    {
        
        RaycastHit cursorRaycast = GetClickTarget();

        if (cursorRaycast.collider.name == islandName) //If the user clicks or drags on the island, we don't care about other factors so it can be shortcut
        {         
            MouseDragMove(cursorRaycast);
        }
        else
        {
            if (cursorRaycast.collider.name == seaName && Input.GetMouseButtonDown(0))
            {
                HandleClickOnSea(cursorRaycast);
            }
        }
    }

    private void HandleClickOnSea(RaycastHit cursorRaycast)
    {
        Debug.Log("Clicked on sea");

        Vector3 playerToTarget = cursorRaycast.point - transform.position; //Can be abstracted later
        playerToTarget.y = 0;

        float distanceToCast = playerToTarget.magnitude;
        //Debug.Log("Distance to point = " + distanceToCast + ", maxDist: " + maxCastDistanceFromTarget + ", minDist" + minCastDistanceFromTarget);

        if (distanceToCast > maxCastDistanceFromTarget)
        {
            Debug.Log("Too far, move toward closest point");

            //Check if point on line between player and target, max distance from target, is on the navmesh, if so move there
            Vector3 moveTarget = cursorRaycast.point + (-playerToTarget.normalized * maxCastDistanceFromTarget);

            RaycastHit hit;

            Physics.Raycast(moveTarget + Vector3.up * 100, Vector3.down, out hit); //+up to offset ray origin from island collider, else it is not detected

            //Instantiate(floatPrefab, hit.point, floatPrefab.transform.rotation);

            if (hit.collider.name == islandName)
            {
                MoveToTarget(hit.point, navPoint);
            }
            else
            {
                NavMeshHit navmeshHit;

                NavMesh.SamplePosition(cursorRaycast.point, out navmeshHit, 50f, islandMeshArea);

                MoveToTarget(navmeshHit.position, navPoint);
            }

            navPoint.GetComponent<NavPoint>().CastOnContact = true;

            //Determine casting target

            Vector3 moveTargetToFishingTarget = cursorRaycast.point - navAgent.destination;

            moveTargetToFishingTarget.y = 0;

            if (moveTargetToFishingTarget.magnitude <= maxCastDistanceFromTarget)
            {
                Debug.Log("Set fishing target");
                fishingTarget = cursorRaycast.point;
            }
            else
            {
                //Calculate new fishing point;
                Debug.Log("Need to get new fishing target");
            }
        }
        //else if (distanceToCast < minCastDistanceFromTarget)
        else if (distanceToCast < minCastDistanceFromTarget)
        {
            Debug.Log("Too close, cast over point");

            Vector3 newTarget = transform.position + (playerToTarget.normalized * minCastDistanceFromTarget);

            StartCoroutine(FishingRoutine(newTarget));
        }
        else
        {
            Debug.Log("In range, casting");
            StartCoroutine(FishingRoutine(cursorRaycast.point));
        }

    }

    //private void HandlePrimaryClick()
    //{
    //    playerAnim.SetBool("b_IsPlayerMoving", true);

    //    RaycastHit cursorRaycast = GetClickTarget();

    //    if (cursorRaycast.collider.name == islandName) //If player clicks on island - 
    //    {
    //        Debug.Log("Clicked island, moving to point");
    //        NavMeshHit discard;

    //        if (NavMesh.SamplePosition(cursorRaycast.point, out discard, 0.5f, islandMeshArea)) //if player click on (or very near) navmesh, move there. Contained within if statement because navmesh is rough in places and causes unpredicatable behaviour otherwise
    //        {
    //            MoveToTarget(cursorRaycast.point, navPoint);
    //        }
    //    }
    //    else
    //    {
    //        if (cursorRaycast.collider.name == seaName) //Else, if the player clicks on or drags over the sea; 
    //        {
    //            if (Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0)) //If the player is holding the mouse button down but has not clicked this frame I.E. they have dragged over the sea;
    //            {
    //                NavMeshHit closestPoint;

    //                NavMesh.FindClosestEdge(cursorRaycast.point, out closestPoint, 1);
    //                MoveToTarget(closestPoint.position, navPoint);
    //            }

    //            else //Else, player has clicked this frame i.e. wants to fish
    //            {
    //                Debug.Log("Clicked sea");
    //                //If player is closer to the clicked point that minimum cast, cast minimum in that direction.
    //                if (GetDistanceToAxis(cursorRaycast.point) < minCastDistanceFromTarget)
    //                {
    //                    Debug.Log("Closer to point than minimum distance");

    //                    Vector3 newTargetOffset = (cursorRaycast.point - transform.position).normalized * minCastDistanceFromTarget;

    //                    Vector3 newTarget = transform.position + newTargetOffset;

    //                    newTarget.y = cursorRaycast.point.y; //Set target Y to sea height

    //                    StartCoroutine(FishingRoutine(newTarget));
    //                }

    //                else
    //                {
    //                    if (GetDistanceToAxis(cursorRaycast.point) > maxCastDistanceFromTarget)
    //                    {
    //                        Debug.Log("Too far to cast, moving");
    //                        ////Calculate point maxdistance from target along vector between player and target

    //                        //Vector3 pointMaxDistanceFromTarget = (cursorRaycast.point - transform.position).normalized * maxCastDistanceFromTarget; //Vector is backwards 
    //                        Vector3 pointMaxDistanceFromTarget = (transform.position - cursorRaycast.point).normalized * maxCastDistanceFromTarget;

    //                        //Check if that point is on island

    //                        Ray ray = new Ray(pointMaxDistanceFromTarget, Vector3.down);
    //                        RaycastHit raycastHit;

    //                        Physics.Raycast(ray, out raycastHit);
    //                        //Debug.Log("RaycastHit: " + raycastHit.collider.name);

    //                        if (raycastHit.collider.name == islandName)
    //                        {
    //                            Debug.Log("In range of island");
    //                            NavMeshHit discard;

    //                            if (NavMesh.SamplePosition(raycastHit.point, out discard, 0.5f, islandMeshArea)) //Move to point if it is on navmesh
    //                            {
    //                                Debug.Log("Moving to point on navMesh");
    //                                MoveToTarget(raycastHit.point, navPoint);
    //                            }

    //                            else
    //                            {
    //                                Debug.Log("Moving to closest edge");
    //                                NavMesh.FindClosestEdge(raycastHit.point, out discard, islandMeshArea); //Else move to closest edge 
    //                                MoveToTarget(discard.position, navPoint);
    //                            }

    //                            //Start fishing when player reaches target
    //                            navPoint.GetComponent<NavPoint>().CastOnContact = true;
    //                            fishingTarget = cursorRaycast.point;
    //                        }

    //                        else //If clicked point is further than maxCastDistance from island, cast as far as possible from closest edge;
    //                        {
    //                            Debug.Log("Too far, moving to cast as close as possible");
    //                            NavMeshHit discard;

    //                            NavMesh.FindClosestEdge(raycastHit.point, out discard, islandMeshArea); //Else move to closest edge 
    //                            MoveToTarget(discard.position, navPoint);
    //                            navPoint.GetComponent<NavPoint>().CastOnContact = true;

    //                            //Calculate point max distance from target along vector between original target and player;
    //                            Vector3 betweenTargetAndCastPoint = cursorRaycast.point - discard.position;

    //                            fishingTarget = discard.position + (betweenTargetAndCastPoint.normalized * maxCastDistanceFromTarget);
    //                        }
    //                    }
    //                    else //Player is in range, can go straight to fishing
    //                    {
    //                        Debug.Log("In range, starting fishing");
    //                        StartCoroutine(FishingRoutine(cursorRaycast.point));
    //                    }
    //                }

    //            }
    //        }
    //    }
    //}

    private float GetDistanceToAxis(Vector3 target)
    {
        return (new Vector2(target.x, target.z) - new Vector2(transform.position.x, transform.position.z)).magnitude;
    }//Returns distance from player to passed point's y-axis

    IEnumerator FishingRoutine(Vector3 target)
    {
        Instantiate(floatPrefab, target, transform.rotation); //For debugging only

        navAgent.ResetPath();
        Debug.Log("Casting");
        MoveToTarget(transform.position, navPoint);

        Vector3 lookAtPoint = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(lookAtPoint);

        playerAnim.Play("Villager@Fishing01");
        playerAnim.SetBool("b_IsPlayerMoving", false);

        //Test code for logic purposes below this point

        bool cancelFishing = false;

        GameObject floatObj = Instantiate(floatPrefab, fishingTarget, transform.rotation);

        //Debug.Log("Launch angle: " + GetLaunchAngle(target));

        //floatObj.transform.LookAt()

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

    private float GetLaunchAngle(Vector3 target)
    {
        //Vector3 targetVectorFlat = new Vector3(target.x,0, target.z) - new Vector3(transform.position.x, 0,transform.position.z);

        //float targetDistance = targetVectorFlat.magnitude;

        ////Formula for angle == A = 0.5 arcsin(gx / V^2)

        //float launchAngle = 0.5f * Mathf.Asin((Physics.gravity.magnitude * targetDistance) / (castSpeed * castSpeed)); //Angle from XZ plane 

        //Vector3 launchVector = targetVectorFlat.normalized * castSpeed;

        ////Get perpendicular vector to launchVector

        //Vector3 rotationAxis =  Vector3.Cross(launchVector, Vector3.up).normalized;

        //return launchAngle;

        Vector3 targetVectorFlat = new Vector3(target.x, 0, target.z) - new Vector3(transform.position.x, 0, transform.position.z);

        float targetDistance = targetVectorFlat.magnitude;

        ////Formula for angle == A = 0.5 arcsin(gx / V^2)
        float launchAngle = 0.5f * Mathf.Asin((Physics.gravity.magnitude * targetDistance) / (castSpeed * castSpeed));

        return launchAngle;

    }

    private void OnTriggerEnter(Collider other) //detect nav points (for animation)
    {
        //Debug.Log("Touch navpoint");
        if (other.gameObject.CompareTag("NavPoint"))
        {
            if (other.GetComponent<NavPoint>().CastOnContact)
            {
                //StartCoroutine(FishingRoutine(other.transform.position));
                StartCoroutine(FishingRoutine(fishingTarget));
            }

            other.GetComponent<NavPoint>().CastOnContact = false;

            playerAnim.SetBool("b_IsPlayerMoving", false);
            navAgent.ResetPath();
        }
    }
}
