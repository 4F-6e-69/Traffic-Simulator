using UnityEngine;

/*public class movment : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}*/

public class NuovoScript : MonoBehaviour
{
    public Vector3 movimento;

    void Start()
    {
    
    }

     void FixedUpdate()
    {
        if(movimento.x<10)

        movimento.x+=0.1f;

        transform.position = movimento;
    }

}
