using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class SetStreetSize : MonoBehaviour
{
    [SerializeField] GameObject cross; 

    [SerializeField] float direction = -1; 

    [SerializeField] float size = 10f;

    
    [SerializeField] GameObject box;

    void Start () {
        MeshRenderer crossMeshRenderer = cross.GetComponent<MeshRenderer>();
        Vector3 crossSize = crossMeshRenderer.bounds.size; 
        
        
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, size);
        

        MeshRenderer ownMeshRenderer = GetComponent<MeshRenderer>();
        Vector3 ownSize = ownMeshRenderer.bounds.size;

        box.transform.localScale = new Vector3(ownSize.x, 3, crossSize.z);

        float newPositionX = (crossSize.x)/2 + (ownSize.x)/2;
        transform.position = new Vector3(newPositionX * direction, transform.position.y, transform.position.z);
        box.transform.position = new Vector3 (transform.position.x, -(box.transform.localScale.y/2 + ownSize.y), transform.position.z);
    }
}
