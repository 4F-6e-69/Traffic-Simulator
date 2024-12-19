using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField] GameObject cameraContaier;
    [SerializeField] float rotationSpeed;

    void Update()
    {
        if (Input.GetAxis("Horizontal") != 0f) {
            cameraContaier.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime * -Input.GetAxis("Horizontal"));
        }
    }
}
