using UnityEngine;

public class MyFlyCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;        // Velocità di movimento
    public float boostMultiplier = 2f;  // Moltiplicatore per movimenti veloci
    public float zoomSpeed = 5f;        // Velocità dello zoom

    [Header("Rotation Settings")]
    public float rotationSpeed = 50f;   // Velocità di rotazione
    public bool allowRotation = true;   // Permette la rotazione della camera

    [Header("Boundaries")]
    public bool useBoundaries = false;  // Usa limiti per la posizione
    public Vector3 minBoundary;         // Limite minimo della posizione
    public Vector3 maxBoundary;         // Limite massimo della posizione


    [Header("Key Rotation Settings")]
    public float keyRotationSpeed = 50f;


    private Vector3 movement;

    void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleKeyRotationX(); // tilting - Rotazione sull'asse X (2 e X)
        HandleKeyRotationY(); // panning - Rotazione sull'asse Y (Q ed E)
        
        if (allowRotation)
        {
            HandleRotation();
        }
        if (useBoundaries)
        {
            ClampPosition();
        }
    }

    void HandleKeyRotationX() { // Rotazione sull'asse X con 2 e X
        
        if (Input.GetKey(KeyCode.Alpha2))
        {
            transform.Rotate(Vector3.right, -keyRotationSpeed * Time.deltaTime, Space.Self); // Ruota verso l'alto
        }

        if (Input.GetKey(KeyCode.X))
        {
            transform.Rotate(Vector3.right, keyRotationSpeed * Time.deltaTime, Space.Self); // Ruota verso il basso
        }
    }

    void HandleKeyRotationY() { // Rotazione sull'asse Y (Q ed E)

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up, -keyRotationSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up, keyRotationSpeed * Time.deltaTime, Space.World);
        }
    }

    // void HandleMovement() {
    //     // Input WASD o frecce direzionali per movimento
    //     float horizontal = Input.GetAxis("Horizontal");
    //     float vertical = Input.GetAxis("Vertical");

    //     // Movimento laterale (asse X/Z) e verticale (asse Y)
    //     movement = new Vector3(horizontal, 0, vertical);
    //     if (Input.GetKey(KeyCode.Space)) movement.y += 1;  // Ascesa
    //     if (Input.GetKey(KeyCode.LeftShift)) movement.y -= 1;  // Discesa

    //     // Boost con Shift sinistro
    //     float speed = Input.GetKey(KeyCode.LeftControl) ? moveSpeed * boostMultiplier : moveSpeed;

    //     // Muove la camera
    //     transform.Translate(movement * speed * Time.deltaTime, Space.World);
    // }

    void HandleMovement() {

        // Input WASD o frecce direzionali per movimento
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Movimento laterale (asse X/Z) in base all'orientamento della camera
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Ignora la componente verticale della direzione
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calcola il movimento basato sugli input
        Vector3 movement = (forward * vertical + right * horizontal);

        // Movimento verticale (Spazio e Left Shift)
        if (Input.GetKey(KeyCode.Space)) movement.y += 1;  // Ascesa
        if (Input.GetKey(KeyCode.LeftShift)) movement.y -= 1;  // Discesa

        // Boost con Left Control
        float speed = Input.GetKey(KeyCode.LeftControl) ? moveSpeed * boostMultiplier : moveSpeed;

        // Muove la camera
        transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }


    void HandleZoom()
    {
        // Scroll del mouse per lo zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll * zoomSpeed;
    }

    void HandleRotation()
    {
        // Rotazione con tasto destro del mouse
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Ruota attorno agli assi Y e X
            transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime, Space.World);
            transform.Rotate(Vector3.right, -mouseY * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    void ClampPosition()
    {
        // Limita la posizione della camera ai confini definiti
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBoundary.x, maxBoundary.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBoundary.y, maxBoundary.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minBoundary.z, maxBoundary.z);
        transform.position = clampedPosition;
    }
}
