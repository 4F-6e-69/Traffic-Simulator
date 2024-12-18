using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] private CarDatabase carData;
    [SerializeField] private float spawnRate = -1;
    private float spawnTimer;

    private void OnEnable() {
        Spawn();
        spawnTimer = 0;
    }

    private void Update() {
        if (spawnTimer > spawnRate && spawnRate > 0) {
            Spawn();
            spawnTimer = 0;
        }

        spawnTimer += Time.deltaTime;
    }

    public void Spawn () {

        var randIndex = Random.Range(1, carData.cars.Count + 1)-1;
        var tempObj = carData.cars[randIndex].carPrefab;

        tempObj = Instantiate(tempObj, new Vector3 (0, 0.75f, 0), Quaternion.identity);
        tempObj.name = "car" + tempObj.GetInstanceID();
    }

    public void Respawn() {
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Agent");

        foreach (GameObject car in cars) {
            CarController carController = car.GetComponent<CarController>();
            carController.DestroyCar();
        }

        Spawn();
    }

}
