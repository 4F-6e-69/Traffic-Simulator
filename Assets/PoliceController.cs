using UnityEngine;
using UnityEngine.AI;

public class PoliceController : MonoBehaviour
{
    [SerializeField] private GameObject policeOfficer;
    private GrassData grassData;
    private NavMeshAgent agent;
    private NavMeshPath path;
    private Animator animator;

    private bool isWalking = false;
    private Vector3 currentTarget;
    private float pauseTime, timer; 
    private int i = -1;

    private void OnEnable() {
        timer = 0;
        pauseTime = Random.Range(5f, 35f);

        currentTarget = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

        animator = policeOfficer.GetComponent<Animator>();
        agent = policeOfficer.GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        agent.speed = Random.Range(0.25f, 0.95f);
        grassData = GameObject.Find("Grass").GetComponent<GrassData>();
    }

    private void Update() {
        timer += Time.deltaTime;
        if (timer > pauseTime) {
            walkToTarget();
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("idle_f_1_150f") && isWalking) {
            animator.CrossFade("Walking", 0.2f);
            isWalking = true;
        } else if (stateInfo.IsName("Walking") && isWalking) {
        // Se l'agente sta camminando, controlla se deve tornare all'animazione di inattività
            if (Vector3.Distance(policeOfficer.transform.position, currentTarget) < 0.00005f) {
                animator.CrossFade("idle_f_1_150f", 0.2f);
                isWalking = false; // Resetta isWalking quando torna all'animazione di inattività
            }
        }
    }

    private void walkToTarget () {
        Debug.Log(i);
        if (isWalking) {
            if (Vector3.Distance(transform.position, currentTarget) < 0.5f) {
                //animator.CrossFadeInFixedTime("Idle", 0.2f);
                isWalking = false;

                i = -1;
                timer = 0;
                pauseTime = Random.Range(0.15f, 0.35f);

                return;
            }
        }

        if (i == -1) {
            currentTarget = grassData.getRandomTarget(transform.position);
            if (agent.CalculatePath(currentTarget, path)) {
                Debug.Log("Path calculated successfully.");
                i = 0; // Inizializza l'indice solo se il percorso è valido
                isWalking = true;
            } else {
                Debug.Log("Failed to calculate path.");
                return; // Esci se il percorso non è valido
            }
        }

        if (path.corners.Length > 0) {
            // Assicurati che l'indice i sia valido
            if (i < path.corners.Length) {
                currentTarget = path.corners[i];
                policeOfficer.transform.LookAt(currentTarget);
                agent.SetDestination(currentTarget);

                if (Vector3.Distance(policeOfficer.transform.position, currentTarget) < 0.05f) {
                    i++; // Incrementa l'indice solo se il corner è stato raggiunto
                }
            } else {
                // Se i è fuori dai limiti, resetta i
                i = -1; // Puoi gestire il termine del percorso qui
            }
        }

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        if (path != null && path.corners.Length > 0) {
            foreach (Vector3 corner in path.corners) {
                Gizmos.DrawSphere(corner, 0.15f);
            }
        }
    }


}
