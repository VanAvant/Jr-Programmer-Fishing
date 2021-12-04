using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerControllerNew : MonoBehaviour
{
    private NavMeshAgent playerAgent;



    //Movement & animation variables
    private float playerSpeed = 8;
    private float playerAcceleration = 120;
    private float playerAngularSpeed = 360;

    public float minDistanceFromCast = 6;
    [SerializeField] Animator playerAnim;

    [SerializeField] GameObject navPointPrefab;
    private GameObject navPoint;

    //camera variables
    [SerializeField] Camera mainCamera;
    Vector3 cameraOffset;

    // Start is called before the first frame update
    void Start()
    {
        playerAgent = GetComponent<NavMeshAgent>();
        playerAgent.speed = playerSpeed;
        playerAgent.acceleration = playerAcceleration;
        playerAgent.angularSpeed = playerAngularSpeed;

        cameraOffset = mainCamera.transform.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandlePrimaryMouseClick();
        }
    }


    private void LateUpdate()
    {
        mainCamera.transform.position = transform.position + cameraOffset;    
    }

    private void HandlePrimaryMouseClick()
    {
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(cursorRay, out hit)) // if ray hits a collider (should always be true)
        {
            if (hit.collider.gameObject.name == "Island") //If it hits the island, move to point
            {
                MovePlayerToTarget(hit.point);
            }

            else
            {
                if (hit.collider.gameObject.name == "Sea") //Else, if it hits the sea:
                {
                    if (GetDistanceToAxis(hit.point) > minDistanceFromCast) // If the player is further from the clicked point that the minimum cast distance:
                    {
                        NavMeshHit closestPoint;
                        NavMesh.SamplePosition(hit.point, out closestPoint, 100f, 1);

                        Vector3 minDistanceFromHit = (hit.point - transform.position).normalized * minDistanceFromCast;

                        Vector3 pointMinDistanceFromHit = hit.point - minDistanceFromHit;

                        if (Vector3.Distance(transform.position, closestPoint.position) < Vector3.Distance(transform.position, pointMinDistanceFromHit)) //If player is closer to edge, move there, otherwise move to minDistanceFromCast units away
                        {
                            MovePlayerToTarget(closestPoint.position);
                        }
                        else
                        {
                            MovePlayerToTarget(pointMinDistanceFromHit);
                        }
                    }
                    else
                    {
                        playerAgent.ResetPath();

                        Debug.Log("Casting");
                        MovePlayerToTarget(transform.position);

                        Vector3 lookAtPoint = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                        transform.LookAt(lookAtPoint);

                        playerAnim.Play("Villager@Fishing01");
                    }
                }

            }
        }
    }

    private void MovePlayerToTarget(Vector3 target) //Uses NavMesh to move player to passed point
    {

        playerAgent.SetDestination(target);
        playerAnim.SetBool("b_IsPlayerMoving", true);

        Object.Destroy(navPoint);
        navPoint = Instantiate(navPointPrefab, target, navPointPrefab.transform.rotation);
    } 

    private float GetDistanceToAxis(Vector3 target)
    {
        return (new Vector2(target.x, target.z) - new Vector2(transform.position.x, transform.position.z)).magnitude;
    }//Returns distance from player to passed point's y-axis

    private void OnTriggerEnter(Collider other) //detect nav points (for animation)
    {
        //Debug.Log("Trigger detected");
        if (other.gameObject.CompareTag("NavPoint"))
        {
            Object.Destroy(other.gameObject);
            playerAnim.SetBool("b_IsPlayerMoving", false);
        }
    }

}
