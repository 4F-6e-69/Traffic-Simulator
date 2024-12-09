using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpawnPointsDatabase", menuName = "Scriptable Objects/SpawnPointsDatabase")]
public class SpawnPointsDatabase : ScriptableObject
{
    public List<SpawnPoint> point;
}

[System.Serializable]
public class SpawnPoint {
    [SerializeField] public string ID;
    [SerializeField] public GameObject carPrefab;
    
}
