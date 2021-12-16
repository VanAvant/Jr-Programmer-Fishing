using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCFish : NavAgentScript
{

    //NPC Spawning variables
    private float spawnMinRadius;
    private float spawnMaxRadius;

    //NPC movement/behaviour variables
    [SerializeField] GameObject navPointPrefab;
    private GameObject navPoint;

    [SerializeField] float targetMaxRadius;

    private Vector3 nextTarget;
    private bool movingToTarget;
    private float swimSpeed;

    private bool isIdle = true;

    void Awake()
    {
        swimSpeed = 8.0f; // Place into child once tested
        navAgent = GetComponent<NavMeshAgent>();
        navPoint = Instantiate(navPointPrefab, GetRandomPosition(), navPointPrefab.transform.rotation);

        spawnMaxRadius = 120;
        spawnMinRadius = 60;
    }

    // Update is called once per frame
    void Update()
    {
        if (isIdle)
        {
            if (!movingToTarget)
            {
                MoveToTarget(GetRandomPosition(), navPoint);
                movingToTarget = true;
            }
        }
    }

    public Vector3 GetRandomPosition()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        randomDirection *= Random.Range(spawnMinRadius, targetMaxRadius);

        Vector3 newPosition = new Vector3(randomDirection.x, transform.position.y, randomDirection.y);

        return newPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Called onTriggerEnter" + other.name);
        //if (other.gameObject.CompareTag("NavPoint") && other.transform.parent.name == this.name)
        if (other.gameObject.CompareTag("NavPoint"))
        {
            //Debug.Log("Touched navPoint");
            //MoveToTarget(GetRandomPosition(), navPoint);
            movingToTarget = false;
        }
    }
}
