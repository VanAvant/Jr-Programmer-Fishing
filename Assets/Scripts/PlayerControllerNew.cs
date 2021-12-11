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

    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        navAgent.speed = playerSpeed;
        navAgent.acceleration = playerAcceleration;
        navAgent.angularSpeed = playerAngularSpeed;

        navPoint = Instantiate(navPointPrefab, navPointPrefab.transform.position, navPointPrefab.transform.rotation);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            HandlePrimaryMouseClick();
        }
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
