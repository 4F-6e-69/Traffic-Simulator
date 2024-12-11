using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class CarAgentPath
{
    private List<Vector3> path;
    public List<Vector3> PATH {

        get{return path;}

        private set{path = value;}

    }

    private NavMeshPath navigatorPath;
    public CarAgentPath (Vector3 spawnPoint) {
        path = new List<Vector3> ();
        path.Add (spawnPoint);

        navigatorPath = new NavMeshPath();
    }

    public void addPath (GameObject navObject) {
        NavMeshAgent navigator = navObject.GetComponent<NavMeshAgent> ();
        navigator.CalculatePath(this.path[1], navigatorPath);
        Vector3[] pathCorners = navigatorPath.corners;
        
        PointManager pointManager;

        List<string> roadsName = new List<string>();
        List<string> roadsType = new List<string>();

        foreach (Vector3 corner in pathCorners)
        {
            GameObject wayPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointManager = wayPoint.AddComponent<PointManager>();
            pointManager.InitializePoint(corner);

            roadsName.Add(pointManager.RoadName);
            roadsType.Add(pointManager.RoadType);
        }

        (roadsName, roadsType) = getAllRoads(roadsName, roadsType, pathCorners);

        //for (int i = 0; i < roadsType.Count; i++) {
        //    Normalize(roadsType[i], roadsName[i]);
        //}

    }

    public Vector3 setDestination () {
        GameObject[] destinationPointsPrefab = GameObject.FindGameObjectsWithTag("Destination");
        var randIndex = Random.Range(1, destinationPointsPrefab.Length + 1)-1;
        Vector3 dest = destinationPointsPrefab[randIndex].transform.position;

        while (isNear(dest, this.path[0], 0.8f)) {
            randIndex = Random.Range(1, destinationPointsPrefab.Length + 1)-1;
            dest = destinationPointsPrefab[randIndex].transform.position;
        }

        return dest;
    }

    private (List<string>, List<string>) getAllRoads (List<string> roadsName, List<string> roadsType, Vector3[] positions) {
        string lastRoad = roadsType[0];

        bool contanct_1 = false;
        bool contanct_2 = false;

        for (int i = 1; i < roadsType.Count; i++) {
            if (roadsType[i] == lastRoad) {
                if (roadsType[i] == "Straight") {
                    string newRoad = "";
                    Vector3 middlePoint = (positions[i] + positions[i-1]) / 2;
                    Collider[] hitColliders = Physics.OverlapSphere(middlePoint, 0.1f);

                    foreach (Collider hit in hitColliders) {
                        if (hit.gameObject.name == lastRoad) {
                            contanct_1 = true;
                            if (contanct_2 == true) {
                                break;
                            }
                        }

                        if (hit.gameObject.name == roadsType[i]) {
                            contanct_2 = true;
                            if (contanct_1 == true) {
                                break;
                            }
                        }

                        if (hit.gameObject.tag == "IntersectionLight") {
                            newRoad = hit.gameObject.name;
                        }

                        
                    }

                    if (!contanct_1 && !contanct_2) {
                        roadsType.Insert(i, "IntersectionLight");
                        roadsName.Insert(i, newRoad);
                        newRoad = "";
                    }

                    contanct_1 = false;
                    contanct_2 = false;
                }

                if (roadsType[i] == "Intersection" || roadsType[i] == "IntersectionLight") {
                    float differeceModule = (positions[i] - positions[i-1]).magnitude;
                    if (differeceModule <= 1.72f) {
                        roadsType.RemoveAt(i);
                    }
                }
            }

            lastRoad = roadsType[i];
        }

        return (roadsName, roadsType);
    }

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

    public static bool isNear (Vector3 pos1, Vector3 pos2, float radius) {
        Vector3 differece = pos1 - pos2;

        if (differece.magnitude < (radius * 2)) {
            return true;
        }

        return false;

    }

}