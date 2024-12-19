using UnityEngine;

public class GrassData : MonoBehaviour
{
    [SerializeField] private GameObject[] joggingPoints;
    [SerializeField] private GameObject[] policeTarget;

    public Vector3 getRandomTarget (Vector3 currentPosition) {
        int index = Random.Range(0, policeTarget.Length);

        while (Vector3.Distance(currentPosition, policeTarget[index].transform.position) < 1f) {
            index = Random.Range(0, policeTarget.Length);
        }
        return policeTarget[index].transform.position;
    }

    public (Vector3, Vector3) getRandomJoggingPoint () {
        int index = Random.Range(0, joggingPoints.Length);
        int index2 = Random.Range(0, joggingPoints.Length);

        while (index2 == index) {
            index2 = Random.Range(0, joggingPoints.Length);
        }
        
        return (joggingPoints[index].transform.position, joggingPoints[index2].transform.position);
    }
}
