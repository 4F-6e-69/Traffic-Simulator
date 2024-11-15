using UnityEngine;

public class GetSize : MonoBehaviour
{

    [SerializeField] GameObject cross;

    [SerializeField] GameObject street;

    void Start() {
        MeshRenderer crossRender = cross.GetComponent<MeshRenderer>();
        Vector3 crossSize = crossRender.bounds.size;

        Debug.Log(crossSize);

        MeshRenderer streetRender = street.GetComponent<MeshRenderer>();
        Vector3 streetSize = streetRender.bounds.size;

        Debug.Log(streetSize);
    }
}
