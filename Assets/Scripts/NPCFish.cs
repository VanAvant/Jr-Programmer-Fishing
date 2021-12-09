using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCFish : NavAgentScript
{
    [SerializeField] GameObject testTracker; //Remove once moving/choosing spawn points is working

    //NPC Spawning variables
    [SerializeField] float spawnMinRadius;
    [SerializeField] float spawnMaxRadius;

    //NPC moving variables
    [SerializeField] GameObject navPointPrefab;
    private GameObject navPoint;
    private Vector3 nextTarget;
    private bool movingToTarget;

    private float swimSpeed;

    // Start is called before the first frame update
    void Awake()
    {
        swimSpeed = 8.0f; // Place into child once tested
        navPoint = Instantiate(navPointPrefab, transform.position, navPointPrefab.transform.rotation);
        navAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!movingToTarget)
        {
            MoveToTarget(GetRandomPosition(), navPoint);
            movingToTarget = true;
        }

        //nextTarget = GetRandomPosition();
        //Instantiate(testTracker, nextTarget, testTracker.transform.rotation);
    }

    

    public Vector3 GetRandomPosition()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        randomDirection *= Random.Range(spawnMinRadius, spawnMaxRadius);

        Vector3 newPosition = new Vector3(randomDirection.x, 0, randomDirection.y);

        return newPosition;
    }

}
