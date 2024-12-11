using UnityEngine;
using System.Collections.Generic;

public class PointManager : MonoBehaviour
{

    private string roadName = "";
    public string RoadName { get { return roadName; } }

    private string roadType = "";
    public string RoadType { get { return roadType; } }

    private Vector3 navMeshAgentPoint;


    public void InitializePoint (Vector3 position) {
        navMeshAgentPoint = position;

        this.transform.position = navMeshAgentPoint;
        this.transform.localScale = Vector3.one * 0.3f;
        this.GetComponent<SphereCollider>().isTrigger = true;

        Renderer renderer = this.GetComponent<Renderer>();
        renderer.material.color = new Color (1, 0, 0);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.1f);
        roadType = hitColliders[0].tag;
        roadName = hitColliders[0].name;
    }


    public Vector3 Normalize () {
        //-0.47 << -2.25 >> -3.24
        Vector3 defaultPoint = getDefault();

        GameObject wayPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        wayPoint.transform.position = defaultPoint;
        wayPoint.transform.localScale = Vector3.one * 0.3f;
        wayPoint.GetComponent<SphereCollider>().isTrigger = true;

        Renderer renderer = wayPoint.GetComponent<Renderer>();
        renderer.material.color = new Color (0, 0, 1);

        return defaultPoint;
    }

    private Vector3 getDefault () {
        if (roadType == "Intersection" || roadType == "IntersectionLight") {
            if (isEnter()) {
                return getTag("IntersectionEnter");
            } else {
                return getTag("IntersectionOut");
            }
        }

        return navMeshAgentPoint;
        
    }
    
    private Vector3 getTag (string currentTag) {   

        GameObject wayContainer = GameObject.Find(roadName).transform.GetChild(0).gameObject;
        int wayCount = wayContainer.transform.childCount;

        Vector3[] wayPoints = new Vector3[wayCount];
        int enterIndex = 0;
        List<float> distances = new List<float>();

        for (int i = 0; i < wayCount; i++) {
            var wayPoint = wayContainer.transform.GetChild(i).gameObject;

            if (wayPoint.tag == currentTag) {
                wayPoints[enterIndex] = wayPoint.transform.position;
                enterIndex++;
            }
        }
        enterIndex = 0;

        foreach (Vector3 wayPoint in wayPoints) {
            distances.Add((wayPoint - this.transform.position).magnitude);
        }

        int minIndex = 0;
        for (int i = 0; i < distances.Count; i++) {
            if (distances[i] < distances[minIndex]) {
                minIndex = i;
            }
        }

        return wayPoints[minIndex];
    }

    private bool isEnter () {
        Transform wayContainer = GameObject.Find(roadName).transform.GetChild(0).transform;

        for (int i = 0; i < wayContainer.childCount; i++) {
            Transform child = wayContainer.GetChild(i);

            if (child.gameObject.tag == "IntersectionEnter") {
                Collider[] colliders = Physics.OverlapSphere(child.gameObject.transform.position, 0.1f);
                foreach (Collider collider in colliders) {
                    if (collider.gameObject.tag == "normalized") {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}
