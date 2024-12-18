using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class CarAgentPath
{
    private List<(Vector3, string, string)> nodes = new List<(Vector3, string, string)>();
    private Vector3[] tempNormalizedPath;

/*
    public PathWay PATH {

        get{return path;}

        private set{path = value;}

    }
*/
    private NavMeshPath navigatorPath;
    public CarAgentPath () {

        //Ricerca del punto di spawn
        (Vector3 spawnPoint, string spawnPointRoadName, string spawnPointRoadType) = SetSpawnPoint();    

        //Ricerca del punto di arrivo
        (Vector3 destinationPoint, string destinationPointRoadName, string destinationPointRoadType) = SetDestination(spawnPoint);

        //Creazione della lista del Percorso
        //path = new PathWay (spawnPointRoadName, spawnPointRoadType, spawnPoint, destinationPointRoadName, destinationPointRoadType, destinationPoint);
        nodes.Add((spawnPoint, spawnPointRoadName, spawnPointRoadType));
        nodes.Add((destinationPoint, destinationPointRoadName, destinationPointRoadType));
        //Creazione del percorso con Ai
        navigatorPath = new NavMeshPath();
    }


    public Vector3[] GetPath() {
        Vector3[] points = new Vector3[nodes.Count];
        for (int i = 0; i < nodes.Count; i++) {
            points[i] = nodes[i].Item1; // Solo la posizione del punto
        }
        return points;
    }


    public Vector3 GetSpawnPoint () {
        return nodes[0].Item1;
    }

    public Vector3 GetDestinationPoint () {
        return nodes[nodes.Count-1].Item1;
    }

    private (Vector3, string, string) SetSpawnPoint () {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("spawn");
        var randIndex = Random.Range(1, spawnPoints.Length)-1;

        Vector3 spawnPoint = spawnPoints[randIndex].transform.position;
        string spawnPointRoadName = spawnPoints[randIndex].transform.parent.name;
        string spawnPointRoadType = spawnPoints[randIndex].transform.parent.tag;

        return (spawnPoint, spawnPointRoadName, spawnPointRoadType);
    }

    private (Vector3, string, string) SetDestination (Vector3 spawnPoint) {
        GameObject[] destinationPointsPrefab = GameObject.FindGameObjectsWithTag("destination");
        var randIndex = Random.Range(1, destinationPointsPrefab.Length + 1)-1;

        Vector3 dest = destinationPointsPrefab[randIndex].transform.position;

        while (IsNear(dest, spawnPoint, 0.8f)) {
            randIndex = Random.Range(1, destinationPointsPrefab.Length + 1)-1;
            dest = destinationPointsPrefab[randIndex].transform.position;
        }

        string destinationPointRoadName = destinationPointsPrefab[randIndex].transform.parent.name;
        string destinationPointRoadType = destinationPointsPrefab[randIndex].transform.parent.tag; 

        return (dest, destinationPointRoadName, destinationPointRoadType);
    }

    public int GetNodesCount () {
        return nodes.Count;
    }

    public Vector3 GetNode (int index) {
        return nodes[index].Item1;
    }

    public bool AddPath (string currentCarName, NavMeshAgent navigator) {
        (GameObject pathAiContainer, GameObject pathRigidPath) = SetPathContainers(currentCarName);
        
        navigator.CalculatePath(GetDestinationPoint(), navigatorPath);
        Vector3[] pathCorners = navigatorPath.corners;

        DrawPoints(pathCorners, Color.red, pathAiContainer);

        List<string> allRoads = new List<string>();
        List<string> allRoadsType = new List<string>(); 

        (allRoads, allRoadsType) = GetAllRoads(pathCorners);
        int lastPathLength = nodes.Count;

        for (int i = 0; i < allRoads.Count; i++) {
            Debug.Log(allRoads[i] + " " + allRoadsType[i]);
        }

        for (int i = 1; i < allRoads.Count; i++) {
            if (allRoadsType[i] == "4_way_intersection") {
                
                if (i >= 1 && i <= allRoads.Count-2) {
                    PathNormalizerV2 pathNormalizerV2 = new PathNormalizerV2(nodes, allRoads[i-1], allRoads[i], allRoads[i+1], 0.0f);
                    if (!pathNormalizerV2.IsNormalized(lastPathLength)) {return false;}
                }
                
            }
            lastPathLength = nodes.Count;
        }
        pathAiContainer.transform.position = new Vector3(0, -5f, 0);
        Vector3[] normalizedPath = new Vector3[nodes.Count];
        for (int i = 0; i < nodes.Count; i++) {
            normalizedPath[i] = nodes[i].Item1 + new Vector3(0, 0.5f, 0);
        }
        DrawPoints(normalizedPath, Color.blue, pathRigidPath);

        return true;
    }

    private void DrawPoints (Vector3[] points, Color color, GameObject pathConatiner) {
        foreach (Vector3 point in points) {
            GameObject wayPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            wayPoint.transform.position = point;
            wayPoint.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            wayPoint.GetComponent<SphereCollider>().isTrigger = true;
            wayPoint.GetComponent<Renderer>().material.color = color;
            wayPoint.transform.parent = pathConatiner.transform;
            wayPoint.name = pathConatiner.name + wayPoint.GetInstanceID();
        }
    }

    public static bool IsNear (Vector3 pos1, Vector3 pos2, float radius) {
        Vector3 differece = pos1 - pos2;

        if (differece.magnitude < (radius * 2)) {
            return true;
        }

        return false;

    }

    private (GameObject, GameObject) SetPathContainers (string currentCarname) {

        GameObject thisContainer = new GameObject();
        thisContainer.name = currentCarname + "_Path";
        thisContainer.transform.parent = GameObject.Find("WayPoints").transform;

        GameObject thisContainerAI = new GameObject();
        thisContainerAI.name = currentCarname + "_PathAI";
        thisContainerAI.transform.parent = thisContainer.transform;

        GameObject thisContainerPhysics = new GameObject();
        thisContainerPhysics.name = currentCarname + "_PathPhysics";
        thisContainerPhysics.transform.parent = thisContainer.transform;

        return (thisContainerAI, thisContainerPhysics);
    }

    private (List<string>, List<string>) GetAllRoads (Vector3[] positions) {
        List<string> roadsName = new List<string>();
        List<string> roadsType = new List<string>();

        foreach (Vector3 position in positions) {
            Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f);
            roadsName.Add(hitColliders[0].gameObject.name);
            roadsType.Add(hitColliders[0].gameObject.tag);
        }

        string lastRoadName = roadsName[0];
        string lastRoadType = roadsType[0];

        bool contanct_1 = false;
        bool contanct_2 = false;

        for (int i = 1; i < roadsType.Count; i++) {

            if (roadsType[i] == lastRoadType) {
                if (roadsType[i] == "straight") {
                    string newRoad = "";
                    Vector3 middlePoint = (positions[i] + positions[i-1]) / 2;
                    Collider[] hitColliders = Physics.OverlapSphere(middlePoint, 0.1f);

                    foreach (Collider hit in hitColliders) {
                        if (hit.gameObject.name == lastRoadName) {
                            contanct_1 = true;
                            if (contanct_2 == true) {
                                break;
                            }
                        }

                        if (hit.gameObject.name == roadsName[i]) {
                            contanct_2 = true;
                            if (contanct_1 == true) {
                                break;
                            }
                        }

                        if (hit.gameObject.tag == "4_way_intersection") {
                            newRoad = hit.gameObject.name;
                        }

                        
                    }

                    if (!contanct_1 && !contanct_2) {
                        roadsType.Insert(i, "4_way_intersection");
                        roadsName.Insert(i, newRoad);
                        newRoad = "";
                    }

                    contanct_1 = false;
                    contanct_2 = false;
                }

                if ((roadsType[i] == "4_way_intersection" /*|| roadsType[i] == "4_way_intersection"*/)) {
                    float differeceModule = (positions[i] - positions[i-1]).magnitude;
                    if (differeceModule <= 1.6f) {
                        roadsName.RemoveAt(i);
                        roadsType.RemoveAt(i);
                    }
                }
            }

            lastRoadName = roadsName[i];
            lastRoadType = roadsType[i];
        }


        return (roadsName, roadsType);
    }

    public void DestroyPath(string carName) {
        for (int i = 0; i < GameObject.Find(carName + "_PathAI").transform.childCount; i++) {
            GameObject.Destroy(GameObject.Find(carName + "_PathAI").transform.GetChild(i).gameObject);
        }
        GameObject.Destroy(GameObject.Find(carName + "_PathAI"));

        for (int i = 0; i < GameObject.Find(carName + "_PathPhysics").transform.childCount; i++) {
            GameObject.Destroy(GameObject.Find(carName + "_PathPhysics").transform.GetChild(i).gameObject);
        }
        GameObject.Destroy(GameObject.Find(carName + "_PathPhysics"));
/*
        for (int i = 0; i < nodes.Count; i++) {
            if (nodes[i].Item3 == "4_way_intersection") {
                GameObject.Find(nodes[i].Item2).GetComponent<IntersectionPathNormalizer>().RemovePath(carName);
            }
        }
*/
        GameObject.Destroy(GameObject.Find(carName + "_Path"));
        nodes.Clear();
    }

}