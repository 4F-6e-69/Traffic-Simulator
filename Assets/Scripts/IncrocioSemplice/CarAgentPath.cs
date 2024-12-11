using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class CarAgentPath
{
    private PathWay path;
    public PathWay PATH {

        get{return path;}

        private set{path = value;}

    }

    private NavMeshPath navigatorPath;
    public CarAgentPath () {

        //Ricerca del punto di spawn
        (Vector3 spawnPoint, string spawnPointRoadName, string spawnPointRoadType) = setSpawnPoint();    

        //Ricerca del punto di arrivo
        (Vector3 destinationPoint, string destinationPointRoadName, string destinationPointRoadType) = setDestination(spawnPoint);

        //Creazione della lista del Percorso
        path = new PathWay (spawnPointRoadName, spawnPointRoadType, spawnPoint, destinationPointRoadName, destinationPointRoadType, destinationPoint);
        //Creazione del percorso con Ai
        navigatorPath = new NavMeshPath();
    }

    public Vector3 getSpawnPoint () {
        return path.SpawnPosition;
    }

    public Vector3 getDestinationPoint () {
        return path.EndPosition;
    }

    private (Vector3, string, string) setSpawnPoint () {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("spawn");
        var randIndex = Random.Range(1, spawnPoints.Length + 1)-1;

        Vector3 spawnPoint = spawnPoints[randIndex].transform.position;
        string spawnPointRoadName = spawnPoints[randIndex].transform.parent.name;
        string spawnPointRoadType = spawnPoints[randIndex].transform.parent.tag;

        return (spawnPoint, spawnPointRoadName, spawnPointRoadType);
    }

    private (Vector3, string, string) setDestination (Vector3 spawnPoint) {
        GameObject[] destinationPointsPrefab = GameObject.FindGameObjectsWithTag("destination");
        var randIndex = Random.Range(1, destinationPointsPrefab.Length + 1)-1;

        Vector3 dest = destinationPointsPrefab[randIndex].transform.position;

        while (isNear(dest, spawnPoint, 0.8f)) {
            randIndex = Random.Range(1, destinationPointsPrefab.Length + 1)-1;
            dest = destinationPointsPrefab[randIndex].transform.position;
        }

        string destinationPointRoadName = destinationPointsPrefab[randIndex].transform.parent.name;
        string destinationPointRoadType = destinationPointsPrefab[randIndex].transform.parent.tag; 

        return (dest, destinationPointRoadName, destinationPointRoadType);
    }

    public bool AddPath (string currentCarName, NavMeshAgent navigator) {
        (GameObject pathAiContainer, GameObject pathRigidPath) = setPathContainers(currentCarName);
        
        navigator.CalculatePath(getDestinationPoint(), navigatorPath);
        Vector3[] pathCorners = navigatorPath.corners;

        foreach (Vector3 corner in pathCorners) {
            GameObject wayPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wayPoint.transform.position = corner;
            wayPoint.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            wayPoint.GetComponent<SphereCollider>().isTrigger = true;
            wayPoint.GetComponent<Renderer>().material.color = Color.red;
            wayPoint.transform.parent = pathAiContainer.transform;
        }

        List<string> allRoads = new List<string>();
        List<string> allRoadsType = new List<string>();

        (allRoads, allRoadsType) = getAllRoads(pathCorners);
        Normalize(allRoads, allRoadsType, pathCorners);

        for (int i = 0; i < allRoads.Count; i++) {
            Debug.Log(allRoads[i] + " " + allRoadsType[i]);
        }

        

        return true;
    }

    public static bool isNear (Vector3 pos1, Vector3 pos2, float radius) {
        Vector3 differece = pos1 - pos2;

        if (differece.magnitude < (radius * 2)) {
            return true;
        }

        return false;

    }

    private (GameObject, GameObject) setPathContainers (string currentCarname) {

        
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

    private (List<string>, List<string>) getAllRoads (Vector3[] positions) {
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

                        if (hit.gameObject.tag == "intersectionLights") {
                            newRoad = hit.gameObject.name;
                        }

                        
                    }

                    if (!contanct_1 && !contanct_2) {
                        roadsType.Insert(i, "intersectionLights");
                        roadsName.Insert(i, newRoad);
                        newRoad = "";
                    }

                    contanct_1 = false;
                    contanct_2 = false;
                }

                if (roadsType[i] == "intersection" || roadsType[i] == "intersectionLights") {
                    float differeceModule = (positions[i] - positions[i-1]).magnitude;
                    if (differeceModule <= 1.72f) {
                        roadsType.RemoveAt(i);
                    }
                }
            }

            lastRoadName = roadsName[i];
            lastRoadType = roadsType[i];
        }

        return (roadsName, roadsType);
    }

    private void Normalize (List<string> roadsName, List<string> roadsType, Vector3[] positions) {
        int count = roadsName.Count;
        int counter = 0;

        for (int i = 0; i < count; i++) {

            if (positions[counter].x == this.getDestinationPoint().x && positions[counter].z == this.getDestinationPoint().z && positions[counter].y == this.getDestinationPoint().y) { break; }

            GameObject currentRoad = GameObject.Find(roadsName[i]);

            if (roadsType[i] == "intersectionLights") {
                PathNormalizer pathNormalizer = currentRoad.GetComponent<PathNormalizer>();
                pathNormalizer.Normalize(positions[counter], positions[counter+1]);
                counter = counter + 2;
            }

            if (roadsType[i] == "intersection") {
                PathNormalizer pathNormalizer = currentRoad.GetComponent<PathNormalizer>();
                pathNormalizer.Normalize(positions[counter], positions[counter+1]);
                counter = counter + 2;
            }
        }
    }
/*
    private void Normalize (string roadType, string roadName) {
        GameObject currentRoad = GameObject.Find(roadName);
        RoadOrganizer roadOrganizer = currentRoad.GetComponent<RoadOrganizer>();

        if (roadType == "IntersectionLight") {
            Vector3[] crossWayPoint = roadOrganizer.setCrossWayPoint();
            Vector3 dest = path[path.Count-1];
            //path.RemoveAt(path.Count-1);

            //path.AddRange(crossWayPoint);

            //path.Add(dest);
        }
        
    }

    
    */

}