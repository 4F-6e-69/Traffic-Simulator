using System.Collections;
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
    private bool go;
    private bool isTooClose = false;

    private NavMeshAgent navigator;
    private CarAgentPath pathWay;
    private Vector3[] path;  // Percorso calcolato da CarAgentPath
    private int currentWaypointIndex = 0;

    private Rigidbody rb; // Aggiunto il riferimento al Rigidbody
    [SerializeField] public float maxSpeed = 4f;
    [SerializeField] public float acceleration = 0.5f;
    private float currentSpeed = 0f;

    private Vector3 smoothDirection; // Direzione smussata per le curve
    
    private Coroutine greenLightCoroutine; // Riferimento alla coroutine attiva
    private bool isGreenLightActive = false; // Flag per indicare la pausa

    private BoxCollider safetyDistanceCollider;
    [SerializeField] public float safetyDistance = 4f; // Distanza di sicurezza
    [SerializeField] float brakingIntensity = 10f; // Intensità del rallentamento
    private float precedencesRadius = 20f;

    private void OnEnable() {
        pathWay = new CarAgentPath();

        Vector3 spawn = pathWay.getSpawnPoint();
        transform.position = spawn  + new Vector3 (0f, 0.3f, 0f);
        transform.LookAt(transform.forward);

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
            path = pathWay.getPath(); // Ottieni il percorso completo come array di punti
            if (path.Length > 0) {
                // Inizializza la direzione smussata con il primo waypoint
                smoothDirection = (path[0] - transform.position).normalized;
            }
        } else {
            DestroyCar();
        } 


        safetyDistanceCollider = gameObject.AddComponent<BoxCollider>();
        safetyDistanceCollider.isTrigger = true;

        // Configura la dimensione e il posizionamento del collider
        safetyDistanceCollider.size = new Vector3(1f, 1f, safetyDistance); // Larghezza, altezza, lunghezza
        safetyDistanceCollider.center = new Vector3(0, 1f, safetyDistance / 2); // Posizionato davanti
    
        go = true;
        rb = GetComponent<Rigidbody>();
        if (rb == null) {
            Debug.LogError("Rigidbody non trovato! Aggiungilo al GameObject.");
            return;
        }

        // Assicurati che il Rigidbody abbia i parametri giusti
        rb.isKinematic = false;
        rb.useGravity = true;

    }

    void OnTriggerStay(Collider other) {
        if (other.CompareTag("Agent")) {
            // Calcola la direzione verso l'altro veicolo
            Vector3 directionToOther = (other.transform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToOther);

            // Se il veicolo è dietro di te (dotProduct < 0), ignora
            if (dotProduct < 0) {
                isTooClose = false; // Non segnalare la vicinanza
            } else {
                isTooClose = true; // Il veicolo davanti è troppo vicino
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Agent")) {
            isTooClose = false;
        }
    }

    private void Update() {

        if (isTooClose) // Rallenta il veicolo o fermalo
                currentSpeed = Mathf.Max(currentSpeed - brakingIntensity * safetyDistance * Time.deltaTime, 0);  
        else MoveVehicle();
        
        if(CarAgentPath.isNear(transform.position, path[path.Count()-1], 0.5f)){
            DestroyCar();
        }
    }

    void OnDrawGizmos() {
        if (safetyDistanceCollider != null) {
            Gizmos.color = Color.blue;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(safetyDistanceCollider.center, safetyDistanceCollider.size);
        }    
    }

    private IEnumerator DisableTrafficLightCheckTemporarily(float duration) {
        isGreenLightActive = true; // Attiva il flag per interrompere il controllo
        yield return new WaitForSeconds(duration); // Aspetta 2.3 secondi
        isGreenLightActive = false; // Riprendi il controllo
    }


    private void MoveVehicle() {

        if (isDestroyed || path == null || path.Length == 0) {
            Debug.LogWarning("Il veicolo non si muove: percorso non valido o distrutto.");
            return;
        }

        if (!isGreenLightActive) {
            int tlc = CheckTrafficLightColor(); // Traffic light color
            switch(tlc){
                case 2: // Yellow
                    currentSpeed = Mathf.Max(currentSpeed - acceleration * brakingIntensity * Time.deltaTime, 0); // rallenta
                    return;
                case 3: // Red
                    currentSpeed = 0;
                    return;
                case 1: // Green
                    StartCoroutine(DisableTrafficLightCheckTemporarily(1.5f)); // Disabilita il controllo del semaforo per pochi secondi
                    break;
            }
        }

        HandlePrecedences();
        
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

    private readonly object lockObject = new object();
    private HashSet<GameObject> previousNearbyVehiclesSet = new HashSet<GameObject>(); // Memorizza l'insieme dei veicoli nel frame precedente
    private float previousAngle = 0f, angle = 0f;

    // Funzione per confrontare se due set sono uguali
    private bool AreSetsEqual(HashSet<GameObject> set1, HashSet<GameObject> set2) {
        if (set1 == null || set2 == null) return false;
        if (set1.Count != set2.Count) return false;

        foreach (var item in set1) {
            if (!set2.Contains(item)) return false;
        }

        return true;
    }

    private void HandlePrecedences() {
    
        LayerMask vehicleLayer = LayerMask.GetMask("Terrein"); // Layer dei veicoli

        // Ottieni tutti i veicoli vicini
        Collider[] nearbyVehicles = Physics.OverlapSphere(transform.position, precedencesRadius, vehicleLayer);
        // Usa un HashSet per evitare duplicati
        HashSet<GameObject> nearbyVehiclesSet = new HashSet<GameObject>();

        foreach (var vehicle in nearbyVehicles) {
            if (vehicle.gameObject != this.gameObject) { // Salta se è il proprio collider
                nearbyVehiclesSet.Add(vehicle.gameObject);
            }
        }

        bool mustYield = false;

        // Decidi la direzione di marcia del veicolo
        // Ad esempio, puoi determinare questa direzione tramite un input o un valore del tipo "direzioneVeicolo" (1 = destra, 0 = dritto, -1 = sinistra)
        int direction = GetVehicleDirection();
    
        Vector3 vehiclePosition = transform.position; // Punto centrale da cui disegnare la direzione
        Vector3 targetDirection = Vector3.zero; // Calcolare la direzione in cui il veicolo deve andare

        switch(direction){
            case 1: 
                // Direzione destra, ma non completamente laterale, quindi più orientata al centro
                targetDirection = transform.forward + transform.right * 0.5f; // Leggera deviazione a destra
                // Debug.Log($"{this.name} deve andare a destra");
                break;
            case 0:
                // Direzione dritta
                targetDirection = transform.forward;
                // Debug.Log($"{this.name} deve andare dritto");
                break;
            case -1:
                // Direzione sinistra, ma non completamente laterale, quindi più orientata al centro
                targetDirection = transform.forward - transform.right * 0.5f; // Leggera deviazione a sinistra
                // Debug.Log($"{this.name} deve andare a sinistra");
                break;
            default:
                break; // Se non c'è direzione valida, esce dalla funzione
        }

        // Disegnare 3 raggi lungo la direzione (senza svanire)
        float rayLength = 2f; // Lunghezza dei raggi
        Color rayColor = Color.yellow; // Colore dei raggi

        // Disegnare i raggi in direzione futura
        for (int i = 0; i < rayLength; i++) {
            // Disegnare i raggi con una leggera variazione per simulare più raggi
            Vector3 offset = new Vector3(0, i * 0.5f - 1f, 0); // Offset per ottenere una distribuzione più spaziosa dei raggi
            Debug.DrawRay(vehiclePosition + offset, targetDirection * rayLength, rayColor);
        }

        if (!AreSetsEqual(nearbyVehiclesSet, previousNearbyVehiclesSet) || previousAngle != angle) {

            // Aggiorna il set precedente solo se è cambiato
            previousNearbyVehiclesSet = nearbyVehiclesSet;
            previousAngle = angle;

            // Esegui il blocco protetto con lock se nearbyVehiclesSet è cambiato
            lock (lockObject) {
                if (nearbyVehiclesSet.Count != 0) {
                    int i = 0;
                    //Debug.Log($"{this.name} ha rilevato nella sfera {nearbyVehiclesSet.Count} veicoli");
                    foreach (var vehicle in nearbyVehiclesSet) {
                        i++; string msg = $"{this.name} - {i}/{nearbyVehiclesSet.Count} - {vehicle.name}";

                        // Calcola il vettore verso il veicolo rilevato
                        Vector3 toOtherVehicle = vehicle.transform.position - transform.position;

                        // Calcola l'angolo rispetto alla destra del veicolo
                        angle = Vector3.SignedAngle(transform.forward, toOtherVehicle, Vector3.up);
                        msg += $" - {angle}";

                        // Se il veicolo deve andare a destra, non deve dare precedenza
                        if (direction == 1) {
                            msg += " - NO PRECEDENZA";
                            continue;
                        }

                        // Se il veicolo deve andare dritto, dà precedenza a chi viene dalla destra
                        if (direction == 0) {
                            if (angle > 0 && angle < 90) { // Il veicolo sulla destra entra nell'incrocio
                                mustYield = true;
                                msg += " - SI PRECEDENZA";
                                // Debug.Log(msg);
                                break;
                            } else {
                                msg += " - NO PRECEDENZA";
                            }
                        }

                        // Se il veicolo deve andare a sinistra, deve dare precedenza a chi viene dalla destra e chi viene di fronte
                        if (direction == -1) {
                            if (angle > 0 && angle < 90 || Mathf.Abs(angle) < 10) { // Il veicolo sulla destra o di fronte sta entrando nell'incrocio                                msg += " - Si PRECEDENZA";
                                // Debug.Log(msg);
                                mustYield = true;
                                break;
                            } else {
                                msg += " - NO PRECEDENZA";
                            }
                        }

                        // Debug.Log(msg);
                    }

                }
            }
        }


        // Se il veicolo ha la precedenza, riduci la velocità
        if (mustYield) {
            // currentSpeed = 0;
            currentSpeed = Mathf.Max(currentSpeed - acceleration * brakingIntensity * Time.deltaTime, 0);
        } else {
            // Se nessun veicolo ha precedenza, continua normalmente
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        }
    }

    // Metodo per determinare la direzione del veicolo (esempio)
    // Ritorna 1 se deve svoltare a destra, 0 se deve andare dritto, -1 se deve svoltare a sinistra
    private int GetVehicleDirection() {

        if(currentWaypointIndex < path.Length-2) {

            // Ottieni il waypoint corrente e il prossimo waypoint
            Vector3 current = path[currentWaypointIndex];
            Vector3 next = path[currentWaypointIndex + 1];
            // Calcola la direzione del vettore tra il punto corrente e il prossimo
            Vector3 currentDirection = (next - current).normalized;
            // Ottieni il prossimo punto per calcolare la direzione futura
            Vector3 future = path[currentWaypointIndex + 2];
            // Calcola la direzione tra il prossimo waypoint e il waypoint successivo
            Vector3 nextDirection = (future - next).normalized;

            // Calcola il prodotto vettoriale tra la direzione attuale e la futura
            float crossProduct = Vector3.Cross(currentDirection, nextDirection).y;

            // Determina la direzione basata sul prodotto vettoriale
            if (crossProduct > 0) return 1; // Destra
            else if (crossProduct < 0) return -1; // Sinistra
            else return 0; // Dritto
        }
        return 1;
    }

    // private void HandlePrecedences() {
        
    //     LayerMask vehicleLayer = LayerMask.GetMask("Vehicle"); // Layer dei veicoli

    //     // Ottieni tutti i veicoli vicini
    //     Collider[] nearbyVehicles = Physics.OverlapSphere(transform.position, precedencesRadius, vehicleLayer);
        
    //     bool mustYield = false;

    //     foreach (var vehicle in nearbyVehicles) {
    //         // Salta se è il proprio collider
    //         if (vehicle.gameObject == this.gameObject) continue;

    //         // Calcola il vettore verso il veicolo rilevato
    //         Vector3 toOtherVehicle = vehicle.transform.position - transform.position;

    //         // Calcola l'angolo rispetto alla destra del veicolo
    //         float angle = Vector3.SignedAngle(transform.forward, toOtherVehicle, Vector3.up);

    //         // Debug dell'angolo
    //         Debug.Log($"Angolo rispetto al veicolo {vehicle.name}: {angle}");

    //         // Visualizza il vettore verso il veicolo con un Gizmo
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawLine(transform.position, vehicle.transform.position);

    //         // Controlla se il veicolo è alla destra (angolo tra 0 e 90 gradi)
    //         if (angle > 0 && angle < 90) {
    //             // Verifica se l'altro veicolo è abbastanza vicino e si sta avvicinando
    //             float distanceToOtherVehicle = toOtherVehicle.magnitude;

    //             // Aggiungi una condizione per considerare solo i veicoli che si stanno avvicinando e sono abbastanza vicini
    //             if (distanceToOtherVehicle < precedencesRadius && Vector3.Dot(toOtherVehicle.normalized, vehicle.transform.forward) > 0.5f) {
    //                 // Il veicolo sulla destra sta entrando nell'incrocio
    //                 Debug.Log($"Dare precedenza al veicolo: {vehicle.name}");
    //                 mustYield = true;
    //                 break;
    //             }
    //         }

    //         // Debug: mostra la distanza tra i veicoli
    //         Debug.Log($"Distanza al veicolo {vehicle.name}: {toOtherVehicle.magnitude}");

    //     }

    //     // Se il veicolo ha la precedenza, riduci la velocità
    //     if (mustYield) {
    //         currentSpeed = Mathf.Max(currentSpeed - acceleration * Time.deltaTime, 0);
    //     } else {
    //         // Se nessun veicolo ha precedenza, continua normalmente
    //         currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
    //     }
    // }


    // private void HandlePrecedences() {
        
    //     float detectionRadius = 10f; // Raggio per rilevare i veicoli vicini
    //     LayerMask vehicleLayer = LayerMask.GetMask("Vehicle"); // Layer dei veicoli

    //     // Ottieni tutti i veicoli vicini
    //     Collider[] nearbyVehicles = Physics.OverlapSphere(transform.position, detectionRadius, vehicleLayer);

    //     foreach (var vehicle in nearbyVehicles) {
    //         // Salta se è il proprio collider
    //         if (vehicle.gameObject == this.gameObject) continue;

    //         // Calcola il vettore verso il veicolo rilevato
    //         Vector3 toOtherVehicle = vehicle.transform.position - transform.position;

    //         // Calcola l'angolo rispetto alla destra del veicolo
    //         float angle = Vector3.SignedAngle(transform.forward, toOtherVehicle, Vector3.up);

    //         // Controlla se il veicolo è alla destra (angolo tra 0 e 90 gradi)
    //         if (angle > 0 && angle < 90) {
    //             // Verifica se l'altro veicolo è abbastanza vicino per richiedere la precedenza
    //             float distanceToOtherVehicle = toOtherVehicle.magnitude;

    //             if (distanceToOtherVehicle < detectionRadius) {
    //                 // Ferma il veicolo corrente per dare la precedenza
    //                 Debug.Log($"Dare precedenza al veicolo: {vehicle.name}");
    //                 currentSpeed = Mathf.Max(currentSpeed - acceleration * Time.deltaTime, 0);
    //                 return;
    //             }
    //         }
    //     }

    //     // Se nessun veicolo ha precedenza, continua normalmente
    //     currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
    // }

    // private void HandlePrecedences(){

    //     float lenght = 15f;

    //     int rayCount = 40; // Numero di raggi
    //     float angleRange = 200f; // Gamma dell'angolo in gradi (totale, quindi sarà diviso a metà)

    //     Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; // Origine del raggio sopra il veicolo
    //     float angleStep = angleRange / (rayCount - 1); // Angolo tra ogni raggio

    //     for (int i = 0; i < rayCount; i++) {
    //         // Calcola l'angolo del raggio corrente
    //         float currentAngle = -angleRange / 2 + i * angleStep;
    //         Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward; // Ruota la direzione in base all'angolo

    //         // Lancia il raycast
    //         if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, lenght)) {
    //             if (hit.collider.CompareTag("Agent")) {

    //                 // Calcola la distanza dall'oggetto rilevato
    //                 float distanceToVehicle = hit.distance;
    //                 Debug.DrawRay(rayOrigin, rayDirection * distanceToVehicle, Color.blue); // Raggio blue per mantere la distanza

    //                 // Rallenta in proporzione alla distanza (più vicino = più rallentamento)
    //                 // float slowDownFactor = Mathf.Clamp01(1f - distanceToVehicle / safetyDistance);
    //                 // currentSpeed = Mathf.Max(currentSpeed - brakingIntensity * slowDownFactor * Time.deltaTime, 0);

    //                 if(distanceToVehicle < safetyDistance){
    //                     currentSpeed = Mathf.Max(currentSpeed - brakingIntensity * safetyDistance * Time.deltaTime, 0);
    //                 }
    //                 Debug.Log($"Veicolo rilevato a {distanceToVehicle} metri. Rallentando.");
    //                 return;
    //             } else {
    //                 Debug.DrawRay(rayOrigin, rayDirection * safetyDistance, Color.yellow); // Raggio giallo per spazio libero
    //             }
    //         } else {
    //             // Debug.DrawRay(rayOrigin, rayDirection * safetyDistance, Color.green); // Raggio verde per spazio libero
    //             // Nessun veicolo rilevato: accelera fino alla velocità massima
    //             currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
    //         }
    //     }

    // }

    private int CheckTrafficLightColor() {

        float rayLength = 1.7f; // Lunghezza ridotta del raggio
        float angleOffset = 50f; // Angolo fisso verso destra
        float verticalOffset = 0.3f; // Altezza bassa del raggio
        float horizontalOffset = 0.6f; // Distanza dal centro
        int rayCount = 30; // Numero di raggi per densità
        float raySpacing = 0.01f; // Distanza tra i raggi paralleli

        // Origine centrale del raggio
        Vector3 rayOrigin = transform.position + Vector3.up * verticalOffset + transform.forward * horizontalOffset;
        // Direzione del raggio centrale inclinata di 10 gradi
        Vector3 baseDirection = Quaternion.Euler(0, angleOffset, 0) * transform.forward;

        for (int i = -rayCount / 2; i <= rayCount / 2; i++) {
            // Calcola un piccolo spostamento orizzontale per ogni raggio
            Vector3 rayDirection = baseDirection + transform.right * (i * raySpacing);

            if (Physics.Raycast(rayOrigin, rayDirection.normalized, out RaycastHit hit, rayLength)) {

                if (hit.collider.CompareTag("TrafficLight")) {
                    Debug.DrawRay(rayOrigin, rayDirection.normalized * hit.distance, Color.red);
                    Light trafficLight = hit.collider.GetComponentInChildren<Light>();

                    if (trafficLight != null) {
                        if(AreColorsSimilar(trafficLight.color, new Color(250,0,0), 0.2f)) // RED
                            return 3;
                        if(AreColorsSimilar(trafficLight.color, new Color(205, 215, 108), 0.2f)) // YELLOW
                            return 2;
                        if(AreColorsSimilar(trafficLight.color, new Color(0,255,0), 0.2f)) // GREEN
                            return 1;
                    }
                } else { // e' stato copito qualosa che non e' un semaforo
                } 
            } else {
                Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.green);
            }
        }
        return 0;
    }   

    private bool AreColorsSimilar(Color color1, Color color2, float tolerance) {
    return Mathf.Abs(color1.r - color2.r) < tolerance &&
           Mathf.Abs(color1.g - color2.g) < tolerance &&
           Mathf.Abs(color1.b - color2.b) < tolerance &&
           Mathf.Abs(color1.a - color2.a) < tolerance;
    }

    public void DestroyCar() {
        isDestroyed = true;
        pathWay.DestroyPath(gameObject.name);
        Destroy(gameObject); 
    }
}


