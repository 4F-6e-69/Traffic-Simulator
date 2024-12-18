using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    private List<Vector3> path_way_points;
    private UnityEngine.AI.NavMeshAgent agentAI; private NavMeshPath destinaitonPath;
    private Vector3 nextDestination; private int destinationIndex;
    [SerializeField] private Vector3 destination = new Vector3 (0f, 0f, 0f);


    [SerializeField] float nextDesstinationSpace = 10f;
    [SerializeField] float forceModule = 2f;

    [SerializeField] float maxVelocity = 30;

    private void Update() {
        if (Mathf.Abs(this.transform.position.x) >= Mathf.Sqrt(Mathf.Pow(destination.x, 2) + Mathf.Pow(nextDesstinationSpace, 2))) {
            if (Mathf.Abs(this.transform.position.z) >= Mathf.Sqrt(Mathf.Pow(destination.z, 2) + Mathf.Pow(nextDesstinationSpace, 2))) {
                Move();
            }
        }
    }

    private void OnEnable() {
        agentAI = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agentAI.enabled = true;
        destinaitonPath = new NavMeshPath();

        agentAI.CalculatePath(destination, destinaitonPath);
        agentAI.enabled = false;

        setPath();
    }

    private void Start() {
        path_way_points = new List<Vector3>();
        destinationIndex = 0;
    }

    private void setPath () {
        foreach (var corner in destinaitonPath.corners) {
            path_way_points.Add(corner);
        }
    }

    private bool isNear () {
        if (Mathf.Abs(this.transform.position.x) <= Mathf.Sqrt(Mathf.Pow(nextDestination.x,2) + Mathf.Pow(nextDesstinationSpace, 2))) {
            if (Mathf.Abs(this.transform.position.z) <= Mathf.Sqrt(Mathf.Pow(nextDestination.z,2) + Mathf.Pow(nextDesstinationSpace, 2))) {
                return true;
            }
        }

        return false;
    }


    public void Move (){

        if (isNear()) {
            this.GetComponent<Rigidbody>().AddForce(nextDestination * -forceModule, ForceMode.Impulse);
            nextDestination = path_way_points[destinationIndex];
            destinationIndex ++;
        }

        this.transform.LookAt(nextDestination);

        if (this.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude <= maxVelocity) {
            this.GetComponent<Rigidbody>().AddForce(nextDestination * forceModule, ForceMode.Impulse);
        }
        
    }
}
