using UnityEngine;

public class CarController : MonoBehaviour
{

    public bool isDestroyed = false;

    private CarAgentPath pathWay;
    private void OnEnable() {
        pathWay = new CarAgentPath();
        
        Vector3 spawn = pathWay.getSpawnPoint();
        Vector3 destination = pathWay.getDestinationPoint();
        Collider[] colliders = Physics.OverlapSphere(spawn  + new Vector3 (0, 0.25f, 0), 0.1f);

        transform.position = spawn  + new Vector3 (0, 0.75f, 0);
        transform.LookAt(transform.forward);
    }

    private void Start() {
        int childCount = transform.childCount;
        GameObject navigatorObject = null;

        for (int i = 0; i < childCount; i++) {
            if (transform.GetChild(i).gameObject.name == "navigator") {
                navigatorObject = transform.GetChild(i).gameObject;
            }
        }

        pathWay.AddPath(gameObject.name, navigatorObject.GetComponent<UnityEngine.AI.NavMeshAgent>());
    }

    public void DestroyCar() {
        isDestroyed = true;
        pathWay.DestroyPath(gameObject.name);
        Destroy(gameObject); 
    }
}


