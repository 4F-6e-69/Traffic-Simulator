using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CarDatabase", menuName = "Scriptable Objects/CarDatabase")]
public class CarDatabase : ScriptableObject
{
    public List<Car> cars;
}

[System.Serializable]
public class Car {
    [SerializeField] public string carName;
    [SerializeField] public int carId;
    [SerializeField] public GameObject carPrefab;
}