using UnityEngine;
using UnityEngine.AI;

public class movimento : MonoBehaviour
{
    public Transform target;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent non trovato su questo GameObject!");
        }
    }

    void Update()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
            Debug.Log("Destinazione impostata a: " + target.position);
        }
        else
        {
            Debug.LogWarning("L'agente non è su un NavMesh valido.");
        }
    }
}



/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class movimento : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    // Update is called once per frame
    void Update()
    {
        agent.destination = player.position;
    }
}
*/
