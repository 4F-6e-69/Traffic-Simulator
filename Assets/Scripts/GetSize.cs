using UnityEngine;

public class GetSize : MonoBehaviour
{
    void Start() {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Vector3 size = renderer.bounds.size;
        Debug.Log(size);
    }
}
