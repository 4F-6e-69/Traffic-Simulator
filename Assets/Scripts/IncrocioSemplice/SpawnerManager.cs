using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] private CarDatabase carData;

    
    private void Start() {
        var randIndex = Random.Range(1, carData.cars.Count+1);
        var tempObj = carData.cars[randIndex].carPrefab;
        //tempObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        tempObj = Instantiate(tempObj);
        tempObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
    }

}
