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

        // Vector3 spawn = pathWay.GetSpawnPoint();
        // transform.position = spawn  + new Vector3 (0f, 0.3f, 0f);
        // transform.LookAt(transform.forward);

        // Vector3 spawn = pathWay.GetSpawnPoint();
        // Vector3 next = pathWay.GetNode(1);
        // // transform.position = spawn  + new Vector3 (0f, 0.2f, 0f);
        // transform.LookAt(transform.forward);
        // // Calcola la direzione dal punto di spawn verso il prossimo nodo
        // Vector3 direction = (next - spawn).normalized;
        // // Posiziona la macchina leggermente indietro e in alto rispetto al punto di spawn
        // transform.position = spawn - direction * 0.3f + new Vector3(0f, 0.5f, 0f); // 1.0f indietro, 0.5f in alto
        // // Orienta la macchina nella direzione del movimento
        // transform.rotation = Quaternion.LookRotation(direction);
        

        // Vector3 spawn = pathWay.GetSpawnPoint();
        // // transform.position = spawn  + new Vector3 (0f, 0.3f, 0f);
        // Vector3 next = pathWay.GetPath()[1];
        // Vector3 direction = (next - spawn).normalized;
        // // Definisci un vettore di riferimento (ad esempio, il forward dell'oggetto o un altro vettore di direzione)
        // Vector3 reference = transform.forward.normalized;
        // // Fattore di riduzione dell'angolo (0 significa completamente `reference`, 1 significa completamente `direction`)
        // float reductionFactor = 0.1f; // Cambia questo valore per regolare la riduzione
        // // Interpola tra i due vettori
        // Vector3 reducedDirection = Vector3.Slerp(reference, direction, reductionFactor);
        // Debug.Log("Reduced Direction: " + reducedDirection);
        // transform.position = spawn - reducedDirection * 2f + new Vector3(0f, 0.2f, 0f);
        // transform.LookAt(transform.forward);
        // transform.rotation = Quaternion.LookRotation(transform.forward);

    }

    private void Start() {

        ConsoleUtils.ClearConsole();


        Vector3 spawn = pathWay.GetSpawnPoint();
        Vector3 next = pathWay.GetNode(1);
        // transform.position = spawn  + new Vector3 (0f, 0.2f, 0f);
        transform.LookAt(transform.forward);
        // Calcola la direzione dal punto di spawn verso il prossimo nodo
        Vector3 direction = (next - spawn).normalized;
        Debug.Log("Spawn: " + spawn + " - Next: " + next + " - Direction: " + direction);
        // Posiziona la macchina leggermente indietro e in alto rispetto al punto di spawn
        transform.position = spawn - direction * 1.5f + new Vector3(0f, 0.5f, 0f);
        // Orienta la macchina nella direzione del movimento
        transform.rotation = Quaternion.LookRotation(direction);

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
            if(lightIDs.Count > 1){
                Status nextStatus = CheckTrafficLight();
                Debug.Log(nextStatus);
                if(nextStatus == Status.GO) {
                    trafficStatus = Status.GO;
                    lightIDs.Clear();
                }
            }
        }

        HandlePrecedences();
        HandleSafetyDistance();

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

    // private void HandleSafetyDistance() {
    //     float safetyDistance = 20f; // Distanza di sicurezza in unità
    //     float brakingIntensity = 1500f; // Intensità del rallentamento

    //     // Direzione di controllo: frontale
    //     Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; // Leggermente sopra la base del veicolo
    //     Vector3 rayDirection = transform.forward;

    //     Debug.DrawRay(rayOrigin, rayDirection * safetyDistance, Color.yellow);

    //     // Lancia un Raycast per rilevare veicoli davanti
    //     if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, safetyDistance)) {
    //         if (hit.collider.CompareTag("Agent")) {

    //             // Calcola la distanza dall'oggetto rilevato
    //             float distanceToVehicle = hit.distance;

    //             // Rallenta in proporzione alla distanza (più vicino = più rallentamento)
    //             // float slowDownFactor = Mathf.Clamp01(1f - distanceToVehicle / safetyDistance);
    //             // currentSpeed = Mathf.Max(currentSpeed - brakingIntensity * slowDownFactor * Time.deltaTime, 0);

    //             if(distanceToVehicle < safetyDistance){
    //                 currentSpeed = Mathf.Max(currentSpeed - brakingIntensity * safetyDistance * Time.deltaTime, 0);
    //             }
    //             Debug.Log($"Veicolo rilevato a {distanceToVehicle} metri. Rallentando.");
    //             return;
    //         }
    //     }

    //     // Nessun veicolo rilevato: accelera fino alla velocità massima
    //     currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
    // }

    private void HandleSafetyDistance() {

        float safetyDistance = 10f; // Distanza di sicurezza
        float brakingIntensity = 3000f; // Intensità del rallentamento

        int rayCount = 30; // Numero di raggi
        float angleRange = 180f; // Gamma dell'angolo in gradi (totale, quindi sarà diviso a metà)

        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; // Origine del raggio sopra il veicolo
        float angleStep = angleRange / (rayCount - 1); // Angolo tra ogni raggio

        for (int i = 0; i < rayCount; i++) {
            // Calcola l'angolo del raggio corrente
            float currentAngle = -angleRange / 2 + i * angleStep; // Da -15° a +15° per 30° totali
            Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward; // Ruota la direzione in base all'angolo

            // Lancia il raycast
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, safetyDistance)) {
                // Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.red); // Raggio rosso per collisione
                // Debug.Log($"Raggio {i}: Ostacolo trovato a distanza {hit.distance}");

                if (hit.collider.CompareTag("Agent")) {

                    // Calcola la distanza dall'oggetto rilevato
                    float distanceToVehicle = hit.distance;
                    Debug.DrawRay(rayOrigin, rayDirection * distanceToVehicle, Color.blue); // Raggio blue per mantere la distanza

                    // Rallenta in proporzione alla distanza (più vicino = più rallentamento)
                    // float slowDownFactor = Mathf.Clamp01(1f - distanceToVehicle / safetyDistance);
                    // currentSpeed = Mathf.Max(currentSpeed - brakingIntensity * slowDownFactor * Time.deltaTime, 0);

                    if(distanceToVehicle < safetyDistance){
                        currentSpeed = Mathf.Max(currentSpeed - brakingIntensity * safetyDistance * Time.deltaTime, 0);
                    }
                    Debug.Log($"Veicolo rilevato a {distanceToVehicle} metri. Rallentando.");
                    return;
                } else {
                    Debug.DrawRay(rayOrigin, rayDirection * safetyDistance, Color.yellow); // Raggio giallo per spazio libero
                }
            } else {
                // Debug.DrawRay(rayOrigin, rayDirection * safetyDistance, Color.green); // Raggio verde per spazio libero
                // Nessun veicolo rilevato: accelera fino alla velocità massima
                currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
            }
        }


    }


    private Status CheckTrafficLight() {

        float rayLength = 1.7f; // Lunghezza ridotta del raggio
        float angleOffset = 50f; // Angolo fisso verso destra
        float verticalOffset = 0.8f; // Altezza bassa del raggio
        float horizontalOffset = 0.6f; // Distanza dal centro
        int rayCount = 30; // Numero di raggi per densità
        float raySpacing = 0.01f; // Distanza tra i raggi paralleli

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

        float rayLength = 1.7f; // Lunghezza ridotta del raggio
        float angleOffset = 50f; // Angolo fisso verso destra
        float verticalOffset = 0.8f; // Altezza bassa del raggio
        float horizontalOffset = 0.6f; // Distanza dal centro
        int rayCount = 30; // Numero di raggi per densità
        float raySpacing = 0.01f; // Distanza tra i raggi paralleli

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


