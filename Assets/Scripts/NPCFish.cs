using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCFish : NavAgentScript
{

    //NPC Spawning variables
    private float spawnMinRadius = 75;
    private float spawnMaxRadius = 65;

    //NPC movement behaviour variables
    [SerializeField] GameObject navPointPrefab;
    protected GameObject navPoint;
    private int minWaitTime = 3;
    private int maxWaitTime = 6;
    private bool movingToTarget;

    private int behaviourDelay = 1;

    //Escape behaviour variables
    private bool fishIsEscaping = false;
    private float resetDistance = 100;
    private Vector3 escapeTarget = Vector3.zero;
    
    static bool fishIsBiting = false;

    //Swimming defaults
    protected float defaultSwimSpeed;
    protected float defaultAcceleration;
    protected float defaultAngularSpeed;

    //fishing variables
    //private int nibblesMax = 5;
    //private int nibblesMin = 3;
    //private int remainingNibbles;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navPoint = Instantiate(navPointPrefab, GetRandomPosition(), navPointPrefab.transform.rotation);

        //spawnMaxRadius = 75;
        //spawnMinRadius = 65;

        transform.position = GetRandomPosition();
        StartCoroutine(DelayDefaultRoutine());
    }

    private void Start()
    {
        defaultSwimSpeed = navAgent.speed;
        defaultAcceleration = navAgent.acceleration;
        defaultAngularSpeed = navAgent.angularSpeed;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    fishIsBiting = !fishIsBiting;
        //}
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    fishIsEscaping = !fishIsEscaping;
        //}
    }

    private IEnumerator DelayDefaultRoutine()
    {
        yield return new WaitForSeconds(behaviourDelay);
        StartCoroutine(FishDefaultRoutine());
    }

    public virtual IEnumerator FishDefaultRoutine()
    {
        while (true) //Convert to gameRunning
        {
            if (fishIsEscaping)
            {
                //Debug.Log("Entering escape routine");
                if (escapeTarget == Vector3.zero)
                {
                    EscapeFromTarget();
                }
            }
            else if (fishIsBiting)
            {
                //Debug.Log("Entering biting routine");
            }
            else
            {
                if (!movingToTarget)
                {
                    movingToTarget = true;
                    yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
                    MoveToTarget(GetRandomPosition(), navPoint);
                }
            }
            yield return null;
        }
    }

    public virtual void ResetFish() //Would like to have this reset fish position closer to island but off screen
    {
        navAgent.speed = defaultSwimSpeed;
        navAgent.acceleration = defaultAcceleration;
        navAgent.angularSpeed = defaultAngularSpeed;

        escapeTarget = Vector3.zero;
        fishIsEscaping = false;

        // 'teleport' to opposite side of island;
        Vector3 newPos = new Vector3(-transform.position.x, transform.position.y, -transform.position.z);
        transform.position = newPos;

        //Debug.Log("Returning to normal behaviour");
    }

    public void EscapeFromTarget()
    {
        navAgent.speed *= 5;
        navAgent.acceleration *= 5;
        navAgent.angularSpeed *= 5;
        Vector3 escapeDirection = transform.position.normalized;
        escapeTarget = escapeDirection * resetDistance;
        MoveToTarget(escapeTarget, navPoint);
        //Debug.Log("Escaping to " + escapeTarget);
    }


    public Vector3 GetRandomPosition(bool maxDistance = false)
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        randomDirection *= Random.Range(spawnMinRadius, spawnMaxRadius);

        Vector3 newPosition = new Vector3(randomDirection.x, transform.position.y, randomDirection.y);

        //Debug.Log("NewRandomPosition: " + newPosition);
        return newPosition;
    }

    private void FishHasTakenBait(GameObject fishingFloat)
    {
        fishIsBiting = true;
        //remainingNibbles = Random.Range(nibblesMin, nibblesMax);

        navAgent.ResetPath();
        
        navAgent.acceleration *= 0.5f;
        navAgent.speed *= 0.5f;
        MoveToTarget(fishingFloat.transform.position, navPoint);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Float"))
        {
            //Run canCatch from fishing float
            collision.gameObject.GetComponent<FishingFloat>().FishHasBitten();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("NavPoint"))
        {
            movingToTarget = false;
            //Debug.Log(gameObject.name + "Touched navpoint");

            if (fishIsEscaping)
            {
                //Debug.Log("Escaped, resetting");
                ResetFish();
            }
        }
        else if (other.gameObject.CompareTag("Shark"))
        {
            //Debug.Log(gameObject.name + "detected shark");
            fishIsEscaping = true;
        }
        else if (other.gameObject.CompareTag("Float"))
        {
            if (other.gameObject.GetComponent<FishingFloat>().bitingFish == null)
            {
                other.gameObject.GetComponent<FishingFloat>().bitingFish = gameObject;
                
                Debug.Log("Float detected, biting");

                FishHasTakenBait(other.gameObject);
            }
           
        }
    }
}
