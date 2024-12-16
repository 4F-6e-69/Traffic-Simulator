using UnityEngine;

public class CarController : MonoBehaviour
{

    public bool isDestroyed = false;
    private SpawnerManager spawnerManager;
    [SerializeField] private GameObject navigatorObject;

    private CarAgentPath pathWay;
    private void OnEnable() {
        pathWay = new CarAgentPath();
        spawnerManager = GameObject.Find("Spawner").GetComponent<SpawnerManager>();
        
        Vector3 spawn = pathWay.getSpawnPoint();
        Vector3 destination = pathWay.getDestinationPoint();
        Collider[] colliders = Physics.OverlapSphere(spawn  + new Vector3 (0, 0.25f, 0), 0.1f);

        transform.position = spawn  + new Vector3 (0, 0.75f, 0);
        transform.LookAt(transform.forward);
        gameObject.tag = "Agent";
    }

    private void Start() {
        bool isPathValid = pathWay.AddPath(gameObject.name, navigatorObject.GetComponent<UnityEngine.AI.NavMeshAgent>());

        if (!isPathValid) {
            DestroyCar();
        }
    }

    public void DestroyCar() {
        isDestroyed = true;
        pathWay.DestroyPath(gameObject.name);
        Destroy(gameObject); 
    }
}


