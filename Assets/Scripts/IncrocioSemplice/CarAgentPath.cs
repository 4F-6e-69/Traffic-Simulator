using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Linq;

public class CarAgentPath
{

    private List<(Vector3, string, string)> nodes;
    private List<string> StringPath;
    //private Vector3[] tempNormalizedPath;

    private NavMeshPath navigatorPath;
    public CarAgentPath () {
        nodes = new List<(Vector3, string, string)>();

        CarPath carPath = GameObject.Find("SimulationManager").GetComponent<CarPath>();
        StringPath = carPath.GetPath();
        int count = 0;
        while (!GameObject.Find(StringPath[0]).GetComponent<RoadData>().IsFree()) {
            StringPath = carPath.GetPath();
            count++;
            if (count > 10) {
                break;
            }
        }

        if (count > 10) {
            return;
        }

        GameObject spawnRoad = GameObject.Find(StringPath[0]);
        GameObject destinationRoad = GameObject.Find(StringPath[StringPath.Count-1]);

        Vector3 spawnPoint = spawnRoad.GetComponent<RoadData>().getSpawnPoint();
        Vector3 destinationPoint = destinationRoad.GetComponent<RoadData>().getDestinationPoint();

        nodes.Add((spawnPoint, spawnRoad.name, spawnRoad.tag));
        nodes.Add((destinationPoint, destinationRoad.name, destinationRoad.tag));


/*
        //Ricerca del punto di spawn
        (Vector3 spawnPoint, string spawnPointRoadName, string spawnPointRoadType) = setSpawnPoint();    

        //Ricerca del punto di arrivo
        (Vector3 destinationPoint, string destinationPointRoadName, string destinationPointRoadType) = setDestination(spawnPoint);

        //Creazione della lista del Percorso
        //path = new PathWay (spawnPointRoadName, spawnPointRoadType, spawnPoint, destinationPointRoadName, destinationPointRoadType, destinationPoint);
        nodes.Add((spawnPoint, spawnPointRoadName, spawnPointRoadType));
        nodes.Add((destinationPoint, destinationPointRoadName, destinationPointRoadType));
        //Creazione del percorso con Ai
        navigatorPath = new NavMeshPath();
        */
    }


    public Vector3[] GetPath() {
        Vector3[] points = new Vector3[nodes.Count];
        for (int i = 0; i < nodes.Count; i++) {
            points[i] = nodes[i].Item1; // Solo la posizione del punto
        }
        return points;
    }


    public Vector3 getSpawnPoint () {
        return nodes[0].Item1;
    }

    public Vector3 getDestinationPoint () {
        return nodes[nodes.Count-1].Item1;
    }

    private (Vector3, string, string) setSpawnPoint () {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("spawn");
        //Debug.Log(spawnPoints.Length);
        var randIndex = Random.Range(0, spawnPoints.Length);
        //Debug.Log(spawnPoints[randIndex].transform.position);
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

    public int GetNodesCount () {
        return nodes.Count;
    }

    public Vector3 GetNode (int index) {
        return nodes[index].Item1;
    }

    public bool AddPath (string currentCarName, NavMeshAgent navigator) {
        (GameObject pathAiContainer, GameObject pathRigidPath) = setPathContainers(currentCarName);

        GameObject previousRoad;
        GameObject currentRoad;
        GameObject nextRoad;
        int lastPathLength = nodes.Count;
        PathNormalizerV2 pathNormalizerV2;

        for (int i = 1; i < StringPath.Count-1; i++) {
            previousRoad = GameObject.Find(StringPath[i-1]);
            currentRoad = GameObject.Find(StringPath[i]);
            nextRoad = GameObject.Find(StringPath[i+1]);

            if (currentRoad.tag == "4_way_intersection" || currentRoad.tag == "3_way_intersection") {
                pathNormalizerV2 = new PathNormalizerV2(nodes, previousRoad.name, currentRoad.name, nextRoad.name, 0.0f);
                //if (!pathNormalizerV2.IsNormalized(lastPathLength)) {return false;}
                lastPathLength = nodes.Count;
            }

            if (currentRoad.tag == "round_curve" || currentRoad.tag == "angle_curve") {
                pathNormalizerV2 = new PathNormalizerV2(nodes, previousRoad.name, currentRoad.name, 0.0f);
                //if (!pathNormalizerV2.IsNormalized(lastPathLength)) {return false;}
                lastPathLength = nodes.Count;
            }

        }
        Vector3[] arrayDiVector3 = nodes.Select(node => node.Item1).ToArray();
        //DrawPoints(arrayDiVector3, Color.blue, pathRigidPath);

        return true;
    }
        /*
        
        navigator.CalculatePath(getDestinationPoint(), navigatorPath);
        Vector3[] pathCorners = navigatorPath.corners;

        DrawPoints(pathCorners, Color.red, pathAiContainer);

        List<string> allRoads = new List<string>();
        List<string> allRoadsType = new List<string>(); 

        (allRoads, allRoadsType) = getAllRoads(pathCorners);
        for (int i = 0; i < allRoads.Count; i++) {
            Debug.Log(allRoads[i]);
        }
        
        int lastPathLength = nodes.Count;

        for (int i = 1; i < allRoads.Count; i++) {
            if (allRoadsType[i] == "4_way_intersection" || allRoadsType[i] == "3_way_intersection") {
                
                if (i <= allRoads.Count-2) {
                    //Debug.Log(allRoads[i-1] + " " + allRoads[i] + " " + allRoads[i+1]);
                    PathNormalizerV2 pathNormalizerV2 = new PathNormalizerV2(nodes, allRoads[i-1], allRoads[i], allRoads[i+1], 0.0f);
                    if (!pathNormalizerV2.IsNormalized(lastPathLength)) {return false;}
                }
                
            }
            if (allRoadsType[i] == "round_curve" || allRoadsType[i] == "angle_curve") {
                //Debug.Log(allRoads[i-1]);
                PathNormalizerV2 pathNormalizerV2 = new PathNormalizerV2(nodes, allRoads[i-1], allRoads[i], 0.0f);
                if (!pathNormalizerV2.IsNormalized(lastPathLength)) {return false;}
            }
            lastPathLength = nodes.Count;
        }
        pathAiContainer.transform.position = new Vector3(0, -5f, 0);
        Vector3[] normalizedPath = new Vector3[nodes.Count];
        for (int i = 0; i < nodes.Count; i++) {
            normalizedPath[i] = nodes[i].Item1 + new Vector3(0, 0.5f, 0);
        }
        DrawPoints(normalizedPath, Color.blue, pathRigidPath);*/

    private void DrawPoints (Vector3[] points, Color color, GameObject pathConatiner) {
        foreach (Vector3 point in points) {
            GameObject wayPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            wayPoint.transform.position = point + new Vector3(0, 0.5f, 0);
            wayPoint.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            wayPoint.GetComponent<SphereCollider>().isTrigger = true;
            wayPoint.GetComponent<Renderer>().material.color = color;
            wayPoint.transform.parent = pathConatiner.transform;
            wayPoint.name = pathConatiner.name + wayPoint.GetInstanceID();
        }
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
/*
    private (List<string>, List<string>) getAllRoads (Vector3[] positions) {
        List<string> roadsName = new List<string>();
        List<string> roadsType = new List<string>();
        Collider[] hitColliders;

        for (int i = 0; i < positions.Length; i++) {
            hitColliders = Physics.OverlapSphere(positions[i], 0.1f);
            if (hitColliders.Length > 0) {
                roadsName.Add(hitColliders[0].gameObject.name);
                roadsType.Add(hitColliders[0].gameObject.tag);
            }
        }

        for (int i = 0; i < positions.Length; i++) {
            //Debug.Log(i);
            if (i > 1 && Vector3.Distance(positions[i], positions[i-1]) > 4) {
                Vector3 prevCalculatePoint = positions[i-1];
                Vector3 calculatePoint = positions[i-1] + 1.55f * (positions[i] - positions[i-1]).normalized;

                //Debug.Log(prevCalculatePoint);
                //Debug.Log(calculatePoint);

                while (Vector3.Distance(calculatePoint, positions[i]) > 0.5f) {
                    Vector3 middlePoint = (prevCalculatePoint + calculatePoint) / 2;
                    hitColliders = Physics.OverlapSphere(middlePoint, 0.1f);
                    if (hitColliders.Length > 0) {
                        if (hitColliders[0].gameObject.layer == 3) {
                            roadsName.Add(hitColliders[0].gameObject.name);
                            roadsType.Add(hitColliders[0].gameObject.tag);
                        }else if (hitColliders[1].gameObject.layer == 3) {
                            roadsName.Add(hitColliders[1].gameObject.name);
                            roadsType.Add(hitColliders[1].gameObject.tag);
                        }
                    }
                    //Debug.Log(roadsName[roadsName.Count-1]);
                    //Debug.Log(roadsType[roadsType.Count-1]);
                    prevCalculatePoint = calculatePoint;
                    calculatePoint = calculatePoint + 1.55f * (positions[i] - calculatePoint).normalized;
                }
                continue;
            }

            

        }

        int index = 1;
        int round_count = 0;
        int count = roadsName.Count;

        while (index < count) {
            if ((roadsType[index] == "4_way_intersection" && roadsType[index-1] == "4_way_intersection") || (roadsType[index] == "3_way_intersection" && roadsType[index-1] == "3_way_intersection") || (roadsType[index] == "angle_curve" && roadsType[index-1 ] == "angle_curve")) {

                roadsName.RemoveAt(index);
                roadsType.RemoveAt(index);
                index--;
                count = roadsName.Count;
            }

            if (roadsType[index] == "round_curve") {
                if (round_count < 3) {
                    round_count++;
                    roadsName.RemoveAt(index);
                    roadsType.RemoveAt(index);
                    index--;
                    count = roadsName.Count;
                }else {
                    round_count = 0;
                }

            }

            index++;
        }


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

                if ((roadsType[i] == "4_way_intersection" || roadsType[i] == "3_way_intersection" || roadsType[i] == "angle_curve")) {
                    Debug.Log(roadsName[i] + " " + roadsType[i]);
                    /*
                    float differeceModule = (positions[i] - positions[i-1]).magnitude;
                    if (differeceModule <= 1.6f) {
                        roadsName.RemoveAt(i);
                        roadsType.RemoveAt(i);
                    }
                    
                }

                if (roadsType[i] == "round_curve") {
                    Debug.Log(roadsName[i] + " " + round_count);
                    /*
                    if (round_count <= 3) {
                        round_count++;
                    }else {
                        round_count = 0;
                        roadsName.RemoveAt(i);
                        roadsType.RemoveAt(i);

                        roadsName.RemoveAt(i-1);
                        roadsType.RemoveAt(i-1);

                        roadsName.RemoveAt(i-2);
                        roadsType.RemoveAt(i-2);
                    }
                    
                }
            }

            lastRoadName = roadsName[i];
            lastRoadType = roadsType[i];
        }

        return (roadsName, roadsType);
    }
*/
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