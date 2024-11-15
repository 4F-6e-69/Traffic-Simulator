using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class SetStreetSize : MonoBehaviour
{
    [SerializeField] GameObject cross; 

    void Start () {
        MeshRenderer crossMeshRenderer = cross.GetComponent<MeshRenderer>();
        Vector3 crossSize = crossMeshRenderer.bounds.size; 
        Vector3 crossLocalScale = cross.transform.localScale;

        MeshRenderer ownMeshRenderer = GetComponent<MeshRenderer>();
        Vector3 ownSize = ownMeshRenderer.bounds.size;

        float rate = crossSize.x/ownSize.x;
        Debug.Log(ownSize);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 10f);
        transform.position = new Vector3(-transform.localScale.z * ownSize.x + crossSize.x/2 , transform.position.y, transform.position.z); 
    }
}
