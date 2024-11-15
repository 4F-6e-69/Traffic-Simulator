using UnityEngine;

public class CarSpawn : MonoBehaviour
{

    public GameObject [] carPrefabs; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        Instantiate(SelectCarPrefabs(), transform);
    }

    private GameObject SelectCarPrefabs () {
        var randomIndex = Random.Range(0, carPrefabs.Length);
        return carPrefabs[randomIndex];
    }
}

