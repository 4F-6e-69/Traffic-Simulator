using UnityEngine;
using UnityEngine.AI;

public enum Status {
    GO,
    STOP,
    SLOW_DOWN
}

public class CarController : MonoBehaviour {

    [HideInInspector] public bool isDestroyed = false;


    [SerializeField] private NavMeshAgent navigator;
    private CarAgentPath pathWay;
    private Vector3[] path;  // Percorso calcolato da CarAgentPath
    private int currentWaypointIndex = 0;

    private Rigidbody rb; // Aggiunto il riferimento al Rigidbody
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 0.5f;
    private float currentSpeed = 0f;


    private Vector3 smoothDirection; // Direzione smussata per le curve

    private void OnEnable() {
        pathWay = new CarAgentPath();

        Vector3 spawn = pathWay.getSpawnPoint();
        transform.position = spawn  + new Vector3 (0, 0.05f, 0);
        transform.LookAt(transform.forward);

    }

    private void Start() {

        bool isPathValid = pathWay.AddPath(gameObject.name, navigator);

        if (isPathValid){
            path = pathWay.GetPath(); // Ottieni il percorso completo come array di punti
            if (path.Length > 0) {
                // Inizializza la direzione smussata con il primo waypoint
                smoothDirection = (path[0] - transform.position).normalized;
            }
        }else{
            DestroyCar();
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null) {
            Debug.LogError("Rigidbody non trovato! Aggiungilo al GameObject.");
            return;
        }

        // Assicurati che il Rigidbody abbia i parametri giusti
        rb.isKinematic = false;
        rb.useGravity = true;

    }



    private void Update() {
        MoveVehicle();
    }


    // private void MoveVehicle() {

    //     float initMaxSpeed = 2;
    //     float maxSpeed = 5;

    //     if (isDestroyed || path == null || path.Length == 0) {
    //         Debug.LogWarning("Il veicolo non si muove: percorso non valido o distrutto.");
    //         return;
    //     }

    //     Debug.Log(path.Length);

    //     // Controlla se il veicolo ha completato il percorso
    //     if (currentWaypointIndex >= path.Length){
    //         Debug.Log("Percorso completato. Il veicolo si ferma.");
    //         maxSpeed = 0; // Ferma il veicolo
    //         return;
    //     }

    //     // Ottieni il waypoint corrente
    //     Vector3 targetWaypoint = path[currentWaypointIndex];

    //     // Calcola la direzione verso il waypoint e la distanza
    //     Vector3 direction = (targetWaypoint - transform.position).normalized;
    //     float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint);

    //     // Debug del movimento
    //     Debug.DrawLine(transform.position, targetWaypoint, Color.green); // Visualizza il percorso corrente
    //     Debug.Log($"Distanza al waypoint {currentWaypointIndex}: {distanceToWaypoint}");

    //     // Se il veicolo è abbastanza vicino al waypoint corrente, passa al successivo
    //     if (distanceToWaypoint < 1f)
    //     {
    //         Debug.Log($"Waypoint {currentWaypointIndex} raggiunto. Passo al successivo.");
    //         currentWaypointIndex++;
    //         return;
    //     }

    //     // Controlla per ostacoli davanti
    //     // Status vehicleStatus = CheckForObstacles();

    //     // switch (vehicleStatus) {
    //     //     case Status.STOP:
    //     //         Debug.Log("Ostacolo rilevato. Il veicolo si ferma.");
    //     //         maxSpeed = 0;
    //     //         return;
    //     //     case Status.SLOW_DOWN:
    //     //         Debug.Log("Ostacolo vicino. Il veicolo rallenta.");
    //     //         maxSpeed = initMaxSpeed * 0.5f;
    //     //         break;
    //     //     case Status.GO:
    //     //         Debug.Log("Percorso libero. Velocità normale.");
    //     //         maxSpeed = initMaxSpeed;
    //     //         break;
    //     // }


    //     // Aggiorna la posizione del veicolo verso il waypoint corrente
    //     transform.position += direction * initMaxSpeed * Time.deltaTime;

    //     // Ruota il veicolo gradualmente verso il waypoint corrente
    //     Quaternion targetRotation = Quaternion.LookRotation(direction);
    //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    // }



    // private Status CheckForObstacles() {
    //     if (raycastAnchor == null) {
    //         Debug.LogWarning("raycastAnchor non è assegnato. Nessun controllo ostacoli effettuato.");
    //         return Status.GO;
    //     }

    //     bool obstacleDetected = false;
    //     bool slowDownDetected = false;

    //     // Calcola l'angolo tra i raggi
    //     float rayAngleStep = raySpacing;

    //     for (int i = 0; i < raysNumber; i++)
    //     {
    //         // Calcola la direzione del raggio
    //         float angle = -rayAngleStep * (raysNumber / 2) + i * rayAngleStep;
    //         Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * raycastAnchor.forward;

    //         // Lancia il raggio
    //         if (Physics.Raycast(raycastAnchor.position, rayDirection, out RaycastHit hit, raycastLength))
    //         {
    //             Debug.DrawRay(raycastAnchor.position, rayDirection * raycastLength, Color.red); // Visualizza il raggio

    //             // Controlla la distanza dell'ostacolo
    //             if (hit.distance < emergencyBrakeThresh) {
    //                 obstacleDetected = true;
    //             } else if (hit.distance < slowDownThresh) {
    //                 slowDownDetected = true;
    //             }

    //             Debug.Log($"Rilevato ostacolo a distanza: {hit.distance}");
    //         }
    //         else
    //         {
    //             Debug.DrawRay(raycastAnchor.position, rayDirection * raycastLength, Color.green); // Visualizza il raggio
    //         }
    //     }

    //     // Ritorna lo stato del veicolo in base ai rilevamenti
    //     if (obstacleDetected)
    //         return Status.STOP;

    //     if (slowDownDetected)
    //         return Status.SLOW_DOWN;

    //     return Status.GO;
    // }

    // private void MoveVehicle() {
    //     if (isDestroyed || path == null || path.Length == 0) {
    //         Debug.LogWarning("Il veicolo non si muove: percorso non valido o distrutto.");
    //         return;
    //     }

    //     // Controlla se il veicolo ha completato il percorso
    //     if (currentWaypointIndex >= path.Length) {
    //         Debug.Log("Percorso completato. Il veicolo si ferma.");
    //         currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.deltaTime, 0);
    //         rb.linearVelocity = Vector3.zero;
    //         return;
    //     }

    //     // Ottieni il waypoint corrente
    //     Vector3 targetWaypoint = path[currentWaypointIndex];

    //     // Calcola la direzione verso il waypoint e la distanza
    //     Vector3 direction = (targetWaypoint - transform.position).normalized;
    //     float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint);

    //     Debug.DrawLine(transform.position, targetWaypoint, Color.green);

    //     // Se abbastanza vicino al waypoint, passa al successivo
    //     if (distanceToWaypoint < 1f) {
    //         currentWaypointIndex++;
    //         return;
    //     }

    //     // Calcola una velocità graduale basata sulla distanza
    //     if (distanceToWaypoint > 5f) {
    //         currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
    //     } else {
    //         currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.deltaTime, maxSpeed / 2f);
    //     }

    //     // Applica il movimento
    //     Vector3 velocity = direction * currentSpeed;
    //     rb.MovePosition(rb.position + velocity * Time.deltaTime);

    //     // Ruota il veicolo verso il waypoint in modo fluido
    //     Quaternion targetRotation = Quaternion.LookRotation(direction);

    //     // Rallenta la rotazione per evitare spigolosità nelle curve
    //     float rotationSpeed = distanceToWaypoint < 3f ? 3f : 10f; // Più lento nelle curve strette
    //     rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed));
    // }

    private void MoveVehicle() {
        if (isDestroyed || path == null || path.Length == 0) {
            Debug.LogWarning("Il veicolo non si muove: percorso non valido o distrutto.");
            return;
        }

        // Controlla se il veicolo ha completato il percorso
        if (currentWaypointIndex >= path.Length) {
            Debug.Log("Percorso completato. Il veicolo si ferma.");
            currentSpeed = Mathf.Max(currentSpeed - acceleration * Time.deltaTime, 0);
            rb.linearVelocity = Vector3.zero;
            return;
        }


        Vector3 targetWaypoint = path[currentWaypointIndex]; // Ottieni il waypoint corrente
        Vector3 rawDirection = (targetWaypoint - transform.position).normalized;  // Calcola la direzione verso il waypoint

        // Interpola la direzione per ottenere curve più morbide
        float interpolationSpeed = 5f; // Velocità di smussamento della direzione
        smoothDirection = Vector3.Lerp(smoothDirection, rawDirection, Time.deltaTime * interpolationSpeed);

        // Calcola la distanza al waypoint
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint);

        Debug.DrawLine(transform.position, targetWaypoint, Color.green);

        // Se abbastanza vicino al waypoint, passa al successivo
        if (distanceToWaypoint < 1f) {
            currentWaypointIndex++;
            return;
        }

        // Calcola la velocità in base alla distanza e all'angolo della curva
        float angle = Vector3.Angle(transform.forward, rawDirection);
        float speedFactor = Mathf.Clamp01(1f - angle / 90f); // Rallenta in base all'angolo
        float targetSpeed = maxSpeed * speedFactor;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 2f); // Transizione graduale alla nuova velocità

        // Muovi il veicolo
        Vector3 velocity = smoothDirection * currentSpeed;
        rb.MovePosition(rb.position + velocity * Time.deltaTime);

        // Ruota il veicolo verso la direzione smussata
Quaternion targetRotation = Quaternion.LookRotation(smoothDirection);

// Incrementa la velocità di rotazione in base alla distanza
float rotationSpeed = distanceToWaypoint < 3f ? 10f : 5f; // Più veloce nelle curve strette
rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed));

    }

    public void DestroyCar() {
        isDestroyed = true;
        pathWay.DestroyPath(gameObject.name);
        Destroy(gameObject); 
    }
}


