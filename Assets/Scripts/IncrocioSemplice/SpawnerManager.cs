using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] private CarDatabase carData;

    private void OnEnable() {
        Spawn(getSpawn());
    }

    
    private Vector3 getSpawn () {
        GameObject[] spawnPointsArray = GameObject.FindGameObjectsWithTag("spawn");
        int randIndex = Random.Range(1, spawnPointsArray.Length+1);

        return spawnPointsArray[randIndex].transform.position;
    }

    private void Spawn (Vector3 position) {
        CarAgentPath carAgentPath = new CarAgentPath(position);

        var randIndex = Random.Range(1, carData.cars.Count+1)-1;
        var tempObj = carData.cars[randIndex].carPrefab;

        tempObj = Instantiate(tempObj, position + new Vector3 (0, 0.75f, 0), Quaternion.identity);
    }

}
