using UnityEngine;

public class RoadOrganizer : MonoBehaviour
{
    [SerializeField] private GameObject[] enterRoads;
    [SerializeField] private GameObject[] exitRoads;

    public Vector3[] setCrossWayPoint () {
        Debug.Log("setCrossWayPoint");
        return new Vector3[] {Vector3.zero, Vector3.zero};
    }
}