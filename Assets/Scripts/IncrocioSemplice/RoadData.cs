using UnityEngine;
using System.Collections.Generic;

public class RoadData : MonoBehaviour
{
    [SerializeField] private GameObject[] contactPoint;
    [SerializeField] private GameObject[] wayPoints;

    [SerializeField] private GameObject trafficLights;

    private List<GameObject> intersectionEnterList = new List<GameObject>();

    public Vector3[] GetContactPoint() {
        Vector3[] contactPointPosition = new Vector3[this.contactPoint.Length];

        for (int i = 0; i < this.contactPoint.Length; i++) {
            contactPointPosition[i] = this.contactPoint[i].transform.position;
        }

        return contactPointPosition;
    }

    public GameObject[] GetWaypoints() {
        return this.wayPoints;
    }

    [ContextMenu("GetContactAngle")]
    public void GetContactAngle() {
        foreach (GameObject currentMain in this.contactPoint) {
            foreach (GameObject iteratedPoint  in this.contactPoint) {
                if (currentMain.transform.position == iteratedPoint.transform.position) {
                    continue;
                }

                float difference_x = iteratedPoint.transform.position.x - currentMain.transform.position.x;
                float difference_z = iteratedPoint.transform.position.z - currentMain.transform.position.z;

                float angle = Mathf.Atan2(difference_z, difference_x) * Mathf.Rad2Deg;

                Debug.Log(currentMain.name + " --> " + iteratedPoint.name + " --> " + angle);
            }

            Debug.Log("--------------------------------");
        }
    }

    private bool HasTrafficLights() {

        if (trafficLights == null) {
            return false;
        }

        return true;
    }
    public bool CanIPass(Vector3 intersectionEnter) {
        if (HasTrafficLights()) {
            TrafficLightsManager trafficLightsManager = trafficLights.GetComponent<TrafficLightsManager>();
            IntersectionState state = trafficLightsManager.GetTrafficLightState();

            if  (state == IntersectionState.None) {
                return false;
            }

            if (state == trafficLightsManager.GetCurrenteAxis(intersectionEnter)) {
                return true;
            }

            return false;
        }

        return true;
    }
    public bool CanIPass(Vector3 start, Vector3 end) {
        return false;
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Enter");
    }
}
