using UnityEngine;

public class RoadData : MonoBehaviour
{
    [SerializeField] private GameObject[] contactPoint;
    [SerializeField] private GameObject[] wayPoints;

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
}
