using UnityEngine;
using UnityEngine.AI;

public class PeopleController : MonoBehaviour
{
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    void Update()
    {
    if (Input.GetButtonDown("Fire1")) // Controlla se il pulsante sinistro del mouse è stato premuto
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 mousePosition = hit.point;
            Debug.Log("Posizione del mouse sulla mappa: " + mousePosition);
            agent.SetDestination(mousePosition);
        }
    }
    }
}
