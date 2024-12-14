using UnityEngine;

public class PathNormalizer : MonoBehaviour
{
    [SerializeField] private GameObject[] wayPoints = new GameObject[2];
    
    public bool isSpawn () {
        foreach (GameObject wayPoint in wayPoints) {
            if (wayPoint.tag == "Spawn") {
                return true;
            }
        }
        return false;
    }
}

