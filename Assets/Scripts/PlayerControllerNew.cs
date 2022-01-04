using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerControllerNew : NavAgentScript
{
    [SerializeField] GameManager gameManager;

    //Movement & animation variables
    //[SerializeField] MeshCollider islandCollider;
    private float playerSpeed = 8;
    private float playerAcceleration = 120;
    private float playerAngularSpeed = 360;

    private float minCastDistanceFromTarget = 5.0f;
    private float maxCastDistanceFromTarget = 25.0f;
    [SerializeField] Animator playerAnim;

    //NavMesh variables
    [SerializeField] GameObject navPointPrefab;
    private GameObject navPoint;
    //private int navPointLayer = 6;
    private float samplePositionDistanceIsland = 0.5f;
    private float samplePositionDistance = 50f;

    private string islandName = "Island";
    private int islandMeshArea = 1;
    private string seaName = "SeaQuad";
    //private int seaMeshArea = 3;

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
                HandlePrimaryClick();
            }
        }
    }

    private RaycastHit GetClickTarget()
    {
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Physics.Raycast(cursorRay, out hit, Mathf.Infinity);

        return hit;

    }

    private void MovePlayerToTarget(RaycastHit cursorRaycast)
    {
        NavMeshHit discard;

        float sampleDistance = samplePositionDistance;

        if (cursorRaycast.collider.name == islandName)
        {
            sampleDistance = samplePositionDistanceIsland;
        }

        if (NavMesh.SamplePosition(cursorRaycast.point, out discard, sampleDistance, islandMeshArea)) //Contained within if statement because navmesh is rough in places due to premade island asset and causes unpredicatable behaviour otherwise
        {
            MoveToTarget(discard.position, navPoint);

            if (Vector3.Distance(transform.position, discard.position) > 1f)
            {
                playerAnim.SetBool("b_IsPlayerMoving", true);
            }
        }
    }

    private void HandlePrimaryClick()
    {     
        RaycastHit cursorRaycast = GetClickTarget();

        if (cursorRaycast.collider.name == islandName) //If the user clicks or drags on the island, we don't care about other factors so it can be shortcut
        {         
            MovePlayerToTarget(cursorRaycast);
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
        //Debug.Log("Clicked on sea");

        Vector3 playerToTarget = cursorRaycast.point - transform.position; //Can be abstracted later
        playerToTarget.y = 0;

        float distanceToCast = playerToTarget.magnitude;

        if (distanceToCast > maxCastDistanceFromTarget)
        {
            //Debug.Log("Too far, move toward closest point");

            //Check if point on line between player and target, max distance from target, is on the navmesh, if so move there
            Vector3 moveTarget = cursorRaycast.point + (-playerToTarget.normalized * maxCastDistanceFromTarget);

            RaycastHit hit;

            Physics.Raycast(moveTarget + Vector3.up * 100, Vector3.down, out hit); //+up to offset ray origin from island collider, else it is not detected

            //Instantiate(floatPrefab, hit.point, floatPrefab.transform.rotation);

            if (hit.collider.name == islandName)
            {
                MovePlayerToTarget(hit);
            }
            else
            {
                NavMeshHit navmeshHit;

                NavMesh.SamplePosition(cursorRaycast.point, out navmeshHit, 50f, islandMeshArea); //Get closest edge to clicked point & move there

                RaycastHit newRaycastHit;

                Physics.Raycast(navmeshHit.position + Vector3.up, Vector3.down, out newRaycastHit); //Create new raycast at predetermined point, so the regular movement function can be used 

                MovePlayerToTarget(newRaycastHit); //Opportunity to set up overrides in MovePlayerToTarget to accept both points and RaycastHits
            }

            navPoint.GetComponent<NavPoint>().CastOnContact = true; //We always want to cast after clicking on the sea

            //Determine casting target

            Vector3 moveTargetToFishingTarget = cursorRaycast.point - navAgent.destination;

            moveTargetToFishingTarget.y = 0;

            if (moveTargetToFishingTarget.magnitude <= maxCastDistanceFromTarget)
            {
                //Debug.Log("Set fishing target");
                fishingTarget = cursorRaycast.point;
            }
            else
            {
                //Calculate new fishing point;
                //Debug.Log("Need to get new fishing target");

                Vector3 newFishingTarget = navAgent.destination + (moveTargetToFishingTarget.normalized * maxCastDistanceFromTarget); //Get point max distance from move target along vector from move target to clicked point
                fishingTarget = newFishingTarget;
            }
        }

        else if (distanceToCast < minCastDistanceFromTarget)
        {
            //Debug.Log("Too close, cast over point");

            Vector3 newTarget = transform.position + (playerToTarget.normalized * minCastDistanceFromTarget);

            fishingTarget = newTarget;
            StartCoroutine(FishingRoutine(fishingTarget));
        }
        else
        {
            //Debug.Log("In range, casting");
            fishingTarget = cursorRaycast.point;
            StartCoroutine(FishingRoutine(fishingTarget));
        }
    }

    IEnumerator FishingRoutine(Vector3 target)
    {
        navAgent.ResetPath();
        Debug.Log("Casting");

        Vector3 lookAtPoint = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(lookAtPoint);

        playerAnim.Play("Villager@Fishing01");
        playerAnim.SetBool("b_IsPlayerMoving", false);

        bool cancelFishing = false;

        GameObject floatObj = Instantiate(floatPrefab, fishingTarget, transform.rotation);

        bool hasReleasedButton = false; 

        while (!cancelFishing)
        {
            if (Input.GetMouseButtonUp(0))
            {
                hasReleasedButton = true;
            }

            if (Input.GetMouseButtonDown(0) && hasReleasedButton)
            {
                cancelFishing = true;
            }
            yield return null;
        }

        Debug.Log("Finished fishing");
        GameObject.Destroy(floatObj);
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
