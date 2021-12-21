using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentScript : MonoBehaviour
{
    protected NavMeshAgent navAgent;

    public void MoveToTarget(Vector3 target, GameObject navPoint) //Uses NavMesh to move player to passed point
    {
        Vector3 targetPoint = GetRaycastAtPosition(target).point;

        navPoint.transform.position = targetPoint;

        navAgent.ResetPath();
        navAgent.SetDestination(targetPoint);


        //Current code
        //navPoint.transform.position = target;

        //navAgent.ResetPath();
        //navAgent.SetDestination(GetRaycastAtPosition(target).point);
        
    }
    public RaycastHit GetRaycastAtPosition(Vector3 point)
    {
        Ray ray = new Ray(new Vector3(point.x, 100, point.z), Vector3.down);
        RaycastHit raycastHit;

        Physics.Raycast(ray, out raycastHit);

        return raycastHit;
    }//Returns the position of the top of the island geometry at a passed point 
}
