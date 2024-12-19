using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;

public enum Status {
    GO,
    STOP,
    SLOW_DOWN,
    GO_OVER
}

public class ConsoleUtils : MonoBehaviour {
    public static void ClearConsole() {
        #if UNITY_EDITOR
        var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
        #endif
    }
}


public class CarController : MonoBehaviour {

    float rayLength = 1.7f; // Lunghezza ridotta del raggio
    float angleOffset = 50f; // Angolo fisso verso destra
    float verticalOffset = 0.8f; // Altezza bassa del raggio
    float horizontalOffset = 0.6f; // Distanza dal centro
    int rayCount = 30; // Numero di raggi per densità
    float raySpacing = 0.01f; // Distanza tra i raggi paralleli


    [HideInInspector] public bool isDestroyed = false;
    [HideInInspector] private bool go;
    private HashSet<int> lightIDs;

    private NavMeshAgent navigator;
    private CarAgentPath pathWay;
    private Vector3[] path;  // Percorso calcolato da CarAgentPath
    private int currentWaypointIndex = 0;

    private Rigidbody rb; // Aggiunto il riferimento al Rigidbody
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float acceleration = 0.5f;
    private float currentSpeed = 0f;

    private Vector3 smoothDirection; // Direzione smussata per le curve
    private Status trafficStatus = Status.GO;

    private void OnEnable() {
        pathWay = new CarAgentPath();

        Vector3 spawn = pathWay.GetSpawnPoint();
        Vector3 next = pathWay.GetNode(1);
        transform.position = spawn  + new Vector3 (0f, 0.2f, 0f);
        transform.LookAt(transform.forward);

        // Calcola la direzione dal punto di spawn verso il prossimo nodo
        Vector3 direction = (next - spawn).normalized;
        // Posiziona la macchina leggermente indietro e in alto rispetto al punto di spawn
        transform.position = spawn - direction * 0.3f + new Vector3(0f, 0.5f, 0f); // 1.0f indietro, 0.5f in alto
        // Orienta la macchina nella direzione del movimento
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Start() {

        ConsoleUtils.ClearConsole();

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++) {
            if (transform.GetChild(i).gameObject.name == "navigator") {
                navigator = transform.GetChild(i).gameObject.GetComponent<NavMeshAgent>();
            }
        }

        if (pathWay.AddPath(gameObject.name, navigator)){ // if(isValidPath)
            path = pathWay.GetPath(); // Ottieni il percorso completo come array di punti
            if (path.Length > 0) {
                // Inizializza la direzione smussata con il primo waypoint
                smoothDirection = (path[0] - transform.position).normalized;
            }
        } else {
            DestroyCar();
        } 
    
        go = true;
        lightIDs = new HashSet<int>();

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
        if(CarAgentPath.IsNear(transform.position, path[path.Count()-1], 0.5f)){
            DestroyCar();
        }
    }
    

    private void MoveVehicle() {
        if (isDestroyed || path == null || path.Length == 0) {
            Debug.LogWarning("Il veicolo non si muove: percorso non valido o distrutto.");
            return;
        }

        // ------------- HandleTrafficLights ----------------------

        int lightID = CheckTrafficLight2();
        if (lightID != -1) {
            // Aggiunge l'ID al set e verifica se il set è cambiato
            bool isAdded = lightIDs.Add(lightID);

            if (isAdded) { // Stampa l'HashSet solo quando si verifica un cambiamento
                Debug.Log($"Traffic Light Count: {lightIDs.Count}, IDs: {string.Join(", ", lightIDs)}");
            }
        }
        // Debug.Log($"Traffic Light Count: {lightIDs.Count}, IDs: {string.Join(", ", lightIDs)}");

        
        if(trafficStatus != Status.GO_OVER) {
            Status currentStatus = CheckTrafficLight();
            if (currentStatus != trafficStatus) {
                Debug.Log(currentStatus);
                trafficStatus = currentStatus;
            }
            switch (trafficStatus) {
                case Status.STOP:
                case Status.SLOW_DOWN:
                    currentSpeed = Mathf.Max(currentSpeed - acceleration * Time.deltaTime, 0);
                    return;
                case Status.GO: // Procedi normalmente
                    break;
            }
        } else {
            if(lightIDs.Count == 2){
                Status nextStatus = CheckTrafficLight();
                Debug.Log(nextStatus);
                if(nextStatus == Status.GO) {
                    trafficStatus = Status.GO;
                    lightIDs.Clear();
                }
            }
        }

        // HandlePrecedences();
        // HandleSafetyDistance();

        // Controlla se il veicolo ha completato il percorso
        if (currentWaypointIndex >= path.Length) {
            if(go){
                Debug.Log("Percorso completato. Il veicolo si ferma."); go = false;
            } 
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


    private void HandlePrecedences() {
        
        float detectionRadius = 10f; // Raggio per rilevare i veicoli vicini
        LayerMask vehicleLayer = LayerMask.GetMask("Vehicle"); // Layer dei veicoli

        // Ottieni tutti i veicoli vicini
        Collider[] nearbyVehicles = Physics.OverlapSphere(transform.position, detectionRadius, vehicleLayer);

        foreach (var vehicle in nearbyVehicles) {
            // Salta se è il proprio collider
            if (vehicle.gameObject == this.gameObject) continue;

            // Calcola il vettore verso il veicolo rilevato
            Vector3 toOtherVehicle = vehicle.transform.position - transform.position;

            // Calcola l'angolo rispetto alla destra del veicolo
            float angle = Vector3.SignedAngle(transform.forward, toOtherVehicle, Vector3.up);

            // Controlla se il veicolo è alla destra (angolo tra 0 e 90 gradi)
            if (angle > 0 && angle < 90) {
                // Verifica se l'altro veicolo è abbastanza vicino per richiedere la precedenza
                float distanceToOtherVehicle = toOtherVehicle.magnitude;

                if (distanceToOtherVehicle < detectionRadius) {
                    // Ferma il veicolo corrente per dare la precedenza
                    Debug.Log($"Dare precedenza al veicolo: {vehicle.name}");
                    currentSpeed = Mathf.Max(currentSpeed - acceleration * Time.deltaTime, 0);
                    return;
                }
            }
        }

        // Se nessun veicolo ha precedenza, continua normalmente
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
    }

    private void HandleSafetyDistance() {
        float safetyDistance = 10f; // Distanza di sicurezza in unità
        float brakingIntensity = 1000f; // Intensità del rallentamento

        // Direzione di controllo: frontale
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; // Leggermente sopra la base del veicolo
        Vector3 rayDirection = transform.forward;

        // Lancia un Raycast per rilevare veicoli davanti
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, safetyDistance)) {
            if (hit.collider.CompareTag("Agent")) {
                // Calcola la distanza dall'oggetto rilevato
                float distanceToVehicle = hit.distance;

                // Rallenta in proporzione alla distanza (più vicino = più rallentamento)
                float slowDownFactor = Mathf.Clamp01(1f - distanceToVehicle / safetyDistance);
                currentSpeed = Mathf.Max(currentSpeed - brakingIntensity * slowDownFactor * Time.deltaTime, 0);

                Debug.Log($"Veicolo rilevato a {distanceToVehicle} metri. Rallentando.");
                return;
            }
        }

        // Nessun veicolo rilevato: accelera fino alla velocità massima
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
    }


    private Status CheckTrafficLight() {

        bool trafficLightDetected = false;

        // Origine centrale del raggio
        Vector3 rayOrigin = transform.position + Vector3.up * verticalOffset + transform.forward * horizontalOffset;
        // Direzione del raggio centrale inclinata di 10 gradi
        Vector3 baseDirection = Quaternion.Euler(0, angleOffset, 0) * transform.forward;

        for (int i = -rayCount / 2; i <= rayCount / 2; i++) {
            // Calcola un piccolo spostamento orizzontale per ogni raggio
            Vector3 rayDirection = baseDirection + transform.right * (i * raySpacing);

            if (Physics.Raycast(rayOrigin, rayDirection.normalized, out RaycastHit hit, rayLength)) {
                Debug.DrawRay(rayOrigin, rayDirection.normalized * hit.distance, Color.red);

                if (hit.collider.CompareTag("TrafficLight")) {
                    Light trafficLight = hit.collider.GetComponentInChildren<Light>();
                    if (trafficLight != null) {
                        if(trafficLight.color == new Color(255,0,0)) // RED
                            return Status.STOP;
                        if(trafficLight.color == new Color(205, 215, 108)) // YELLOW
                            return Status.SLOW_DOWN;
                        if(trafficLight.color == new Color(0, 255, 0)) // GREEN
                            return Status.GO_OVER;
                    }
                } else {
                } trafficLightDetected = true;
            } else {
                Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.green);
            }
        }
        return trafficLightDetected ? Status.SLOW_DOWN : Status.GO;
    }


    private int CheckTrafficLight2() {

        int lightID = -1;

        // Origine centrale del raggio
        Vector3 rayOrigin = transform.position + Vector3.up * verticalOffset + transform.forward * horizontalOffset;
        // Direzione del raggio centrale inclinata di 10 gradi
        Vector3 baseDirection = Quaternion.Euler(0, angleOffset, 0) * transform.forward;

        for (int i = -rayCount / 2; i <= rayCount / 2; i++) {
            // Calcola un piccolo spostamento orizzontale per ogni raggio
            Vector3 rayDirection = baseDirection + transform.right * (i * raySpacing);

            if (Physics.Raycast(rayOrigin, rayDirection.normalized, out RaycastHit hit, rayLength)) {

                if (hit.collider.CompareTag("TrafficLight")) {
                    Light trafficLight = hit.collider.GetComponentInChildren<Light>();
                    
                    if (trafficLight != null) {
                        lightID = trafficLight.GetInstanceID();
                    }
                } 
            } 
        }
        return lightID;
    }


    public void DestroyCar() {
        isDestroyed = true;
        pathWay.DestroyPath(gameObject.name);
        Destroy(gameObject); 
    }
}


