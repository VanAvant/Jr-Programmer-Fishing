using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //[SerializeField] GameObject focalPoint; //Currently unused
    [SerializeField] GameObject ocean;      //Currently unused
    [SerializeField] GameObject island;
    private Collider islandCollider;        


    public GameObject testball; //Delete once movement worked out
    private float heightOffset;             //Currently unused

    //Camera variables
    [SerializeField] Camera mainCamera;    
    private Vector3 cameraOffset;
    private float cameraZoomSpeed = 250;
    private float cameraRotateSpeed = 80;
    private int cameraRotateDirection;

    //Movement & animation variables
    public float playerSpeed = 8;
    private Coroutine movementCoroutine;
    private float maxDistanceFromShore = 20;
    [SerializeField] Animator playerAnim;

    // Start is called before the first frame update
    void Start()
    {
        islandCollider = island.GetComponent<MeshCollider>();


        //Calculate Y offset for movement based on player collider size
        //heightOffset = GetComponent<Collider>().bounds.size.y /2;

        SetCameraOffset();

    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            GetMouseInput();
        }

            // Update camera

            if (Input.mouseScrollDelta.y != 0)
        {
            cameraOffset += mainCamera.transform.forward * Time.deltaTime * cameraZoomSpeed * Input.mouseScrollDelta.y;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        mainCamera.transform.position = transform.position + cameraOffset;
        RotateCamera(GetCameraRotateDirection());
    }

    private int GetCameraRotateDirection()
    {
        int direction = 0;

        if (Input.GetKey(KeyCode.E))
        {
            direction = 1;
        }
        else
        {
            if (Input.GetKey(KeyCode.Q))
            {
                direction = -1;
            }
        }
        return direction;
    }

    void RotateCamera(int direction)
    {
        mainCamera.transform.RotateAround(transform.position, Vector3.up, direction * Time.deltaTime * cameraRotateSpeed);
        SetCameraOffset();
    }

    void SetCameraOffset()
    {
        cameraOffset = mainCamera.transform.position - transform.position;
    }





    private void GetMouseInput()
    {
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition); 
        RaycastHit hit;

        if (Physics.Raycast(cursorRay, out hit)) //, 100f
        {
            if (hit.collider.gameObject.name == "Island")
            {
                //Instantiate(testball, hit.point, testball.transform.rotation); //Test function
                if (hit.point.y < 8.0f)
                {
                    Vector3 moveTarget = hit.point;

                    Instantiate(testball, moveTarget, testball.transform.rotation); //Test function

                    try
                    {
                        StopCoroutine(movementCoroutine);
                    }
                    catch
                    {
                        //Debug.Log("Nothing to stop");
                    }
                    playerAnim.SetBool("b_PlayerIsMoving", true);
                    movementCoroutine = StartCoroutine(MovePlayerToTarget(moveTarget).GetEnumerator());
                }
            }
            else
            {
                Debug.Log("Hit " + hit.collider.gameObject.name);

                if (GetDistanceToAxis(hit.point) > maxDistanceFromShore) //If the player is too far from the ocean point they clicked
                {
                    Debug.Log("too far, moving toward point");
                    StartCoroutine(MoveTowardCastingPoint(hit.point));
                }
                else
                {

                    Debug.Log("Starting fishing (close)");
                    StartCoroutine(CastFishingLine());
                }

            }
        }
    }

    private float GetDistanceToAxis(Vector3 target)
    {
        return (new Vector2 (target.x, target.z) - new Vector2 (transform.position.x, transform.position.z)).magnitude;
    }

    IEnumerator MoveTowardCastingPoint(Vector3 target)
    {
        float distance = GetDistanceToAxis(target);

        Debug.Log("Distance: " + distance);

        Coroutine movingToClosestPoint =  StartCoroutine(MovePlayerToTarget(target).GetEnumerator());

        while (distance > maxDistanceFromShore)
        {
            //Debug.Log("Moving to closest point:" + movingToClosestPoint + ", distance: " + distance);
            distance = GetDistanceToAxis(target);
            yield return null;
        }

        StopCoroutine(movingToClosestPoint);

        Debug.Log("Trying to start casting");
        StartCoroutine(CastFishingLine());
    }

    IEnumerable MovePlayerToTarget(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.5f)
        {
            transform.LookAt(new Vector3(target.x, transform.position.y, target.z)); //Replace with rotate over time
            transform.position = Vector3.MoveTowards(transform.position, target, playerSpeed * Time.deltaTime);

            yield return null;
        }
        yield break;
    }

    IEnumerator CastFishingLine()
    {
        //Debug.Log("Started fishing");

        bool hasClicked = false;

        while (!hasClicked)
        {
            if (Input.GetMouseButtonDown(1) || GetComponent<Rigidbody>().velocity.magnitude > 0.1f)
            {
                hasClicked = true;
            }

            yield return null;
        }

        Debug.Log("Finished Fishing");
        yield break;
    }


}

