using UnityEngine;
using UnityEngine.AI;

public class movimento : MonoBehaviour
{
    Transform target;
    private GameObject targetContainer; int children;
    private float timer = 0;
    private float timeToChangeTarget = 10;  
    private NavMeshAgent agent;
    private bool isArrived = true;

    public bool IsArrived { get { return isArrived; } }
    void Start()
    {
        targetContainer = GameObject.Find("Targets");
        children = targetContainer.transform.childCount;
        target = targetContainer.transform.GetChild(Random.Range(0, children+1)).transform;
        timeToChangeTarget = Random.Range(Random.Range(10, 100), Random.Range(190, 500));

        agent = GetComponent<NavMeshAgent>();
        if (Vector3.Distance(target.position, gameObject.transform.position) > 0.2f) {
            isArrived = false;
        }
    }

    void Update(){
        if (target != null && target.transform.position != gameObject.transform.position) {
            if (Vector3.Distance(target.position, gameObject.transform.position) > 0.05f) {
                isArrived = false;
                agent.SetDestination(target.position);
                return;
            }else {
                isArrived = true;
                timer += Time.deltaTime;
                if (timer >= timeToChangeTarget) {
                    target = targetContainer.transform.GetChild(0).transform;
                    while (Vector3.Distance(target.position, gameObject.transform.position) < 0.2f) {
                        target = targetContainer.transform.GetChild(Random.Range(0, children+1)).transform;
                    }
                    timeToChangeTarget = Random.Range(Random.Range(10, 100), Random.Range(190, 500));
                    timer = 0;
                    isArrived = false;
                    return;
                }
            }
        }else {
            target = targetContainer.transform.GetChild(0).transform;
            return;
        }
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Target") {
            target = other.gameObject.transform;
        }
    }

    [SerializeField] private int peopleCount = 10;

    [ContextMenu("Create Agents")]
    public void CreateAgents() {

        for (int i = 0; i < peopleCount; i++) {
            var temp = Instantiate(gameObject, new Vector3(Random.Range(-16, -12), 0, Random.Range(-5, 13)), Quaternion.identity);
            temp.transform.parent = gameObject.transform.parent;
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
