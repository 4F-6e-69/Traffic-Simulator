using UnityEngine;
using System.Collections.Generic;
public enum Direction {
    NONE,
    LEFT,
    RIGHT,
    STRAIGHT
}

public class PathNormalizerV2
{
    private GameObject[] waypoints;
    private Vector3[] contactPoint;
    private string currentRoadName;
    private string currentRoadType;
    private Vector3[] tempPath;
    private List<(Vector3, string, string)> path;

    int additionalPathLength;

    public PathNormalizerV2 (List<(Vector3, string, string)> path, string previous, string current, float spunkRate) {
        this.path = path;
        additionalPathLength = -1;

        GameObject roadData = GameObject.Find(current);
        waypoints = roadData.GetComponent<RoadData>().GetWaypoints();
        contactPoint = roadData.GetComponent<RoadData>().GetContactPoint();
        currentRoadName = current;
        currentRoadType = roadData.tag;
        NormalizeSingleRoad(previous, spunkRate);
    }

    public PathNormalizerV2 (List<(Vector3, string, string)> path, string previous, string current, string next, float spunkRate) {
        this.path = path;
        additionalPathLength = -1;

        GameObject roadData = GameObject.Find(current);
        contactPoint = roadData.GetComponent<RoadData>().GetContactPoint();
        waypoints = roadData.GetComponent<RoadData>().GetWaypoints();
        currentRoadName = current;
        currentRoadType = roadData.tag;

        NormalizeCrossRoad(previous, next, spunkRate);
    }
        

    private void NormalizeSingleRoad(string previous, float spunkRate) {
        // normalizzazione su strada ad un unico ingresso
        // --rettilinei
        // --curve tonde
        // --curve ad angolo retto 


        if (currentRoadType == "round_curve") {
            ///Debug.Log("round_curve");
            this.tempPath = RoundeCurve(previous, spunkRate);
            additionalPathLength = 4;
        }
        if (currentRoadType == "straight_road") {
            //Debug.Log("straight_road");
        }
        if (currentRoadType == "angle_curve") {
            //Debug.Log("angle_curve");
            this.tempPath = AngleCurve(previous, spunkRate);
            additionalPathLength = 4;
        }

        UpdatePath();
    }

    private Vector3[] RoundeCurve(string previous, float spunkRate) {
        GameObject previousRoad = GameObject.Find(previous);
        float minDistance = Mathf.Infinity;
        int index = 0;
        int i = 0;
        
        foreach (GameObject waypoint in waypoints) {
            if (waypoint.tag == "intersectionEnter") {
                if (Vector3.Distance(previousRoad.transform.position, waypoint.transform.position) < Mathf.Abs(minDistance)) {
                    minDistance = Vector3.Distance(previousRoad.transform.position, waypoint.transform.position);
                    index = i;
                }
            }
            i++;
        }

        Vector3 [] newPath = new Vector3[4];

        if (index == 3 || index == 4) {
            newPath[0] = waypoints[3].transform.position;
            newPath[1] = waypoints[2].transform.position;
            newPath[2] = waypoints[1].transform.position;
            newPath[3] = waypoints[0].transform.position;
            Debug.Log("3-2-1-0");

        }else if (index == 5 || index == 0) {
            newPath[0] = waypoints[5].transform.position;
            newPath[1] = waypoints[6].transform.position;
            newPath[2] = waypoints[7].transform.position;
            newPath[3] = waypoints[4].transform.position;
            Debug.Log("5-6-7-4");

        }else {
            return new Vector3[0];
        }

        return newPath;
    }

    private Vector3[] AngleCurve(string previous, float spunkRate) {
        GameObject previousRoad = GameObject.Find(previous);
        float minDistance = Mathf.Infinity;
        int index = 0;
        int i = 0;
        

        foreach (GameObject waypoint in waypoints) {
            if (waypoint.tag == "intersectionEnter") {
                if (Vector3.Distance(previousRoad.transform.position, waypoint.transform.position) < minDistance) {
                    minDistance = Vector3.Distance(previousRoad.transform.position, waypoint.transform.position);
                    index = i;
                }
            }
            i++;
        }

        Vector3 [] newPath = new Vector3[4];

        if (index == 7 || index == 5) {
            newPath[0] = waypoints[7].transform.position;
            newPath[1] = waypoints[4].transform.position;
            newPath[2] = waypoints[1].transform.position;
            newPath[3] = waypoints[0].transform.position;
            Debug.Log("7-4-1-0");

        }else if (index == 2 || index == 0) {
            newPath[0] = waypoints[2].transform.position;
            newPath[1] = waypoints[3].transform.position;
            newPath[2] = waypoints[6].transform.position;
            newPath[3] = waypoints[5].transform.position;
            Debug.Log("2-3-6-5");

        }else {
            return new Vector3[0];
        }

        return newPath;
    }

    private void NormalizeCrossRoad(string previous, string next, float spunkRate) {
        // normalizzazione su strada a più ingressi
        // --incroci a tre vie
        // --incroci a quattro vie
        // --rotonde

        int contactPointIndex = -1;
        Direction direction = Direction.NONE;
        
        if (currentRoadType == "3_way_intersection") {
            (contactPointIndex,direction) = GetThreeWayDirection(previous, next);
        }else if (currentRoadType == "4_way_intersection"  || currentRoadType == "roundabout") {
            (contactPointIndex,direction) = GetDirection(previous, next);  
        }

        int currentLength = path.Count;
        int spin = 0; 

        if (direction == Direction.STRAIGHT) {
            additionalPathLength = 4;
            spin = 2;

        }else if (direction == Direction.LEFT) {
            additionalPathLength = 5;
            spin = 3;

        }else if (direction == Direction.RIGHT) {
            additionalPathLength = 3;
            spin = 1;

        }

        Debug.Log(currentRoadType + "direction: " + direction);

        if (currentRoadType == "3_way_intersection") {this.tempPath =  ThreeWayIntersection(contactPoint[contactPointIndex],direction, spunkRate);}
        if (currentRoadType == "4_way_intersection") {this.tempPath =  FourWayIntersection(contactPoint[contactPointIndex], direction, spunkRate);}
        if (currentRoadType == "roundabout") {additionalPathLength = spin * 3; this.tempPath =  Roundabout(contactPoint[contactPointIndex], direction, spunkRate);}

        UpdatePath();
    }

    private (int, Direction) GetDirection(string previous, string next) {
        GameObject previousRoad = GameObject.Find(previous);
        GameObject nextRoad = GameObject.Find(next);

        Vector3 previousRoadContactPoint = Vector3.zero;
        Vector3 nextRoadContactPoint = Vector3.zero;
        
        foreach (Vector3 pointObject in this.contactPoint) {
            Collider[] colliders = Physics.OverlapSphere(pointObject, 1f);
            foreach (Collider collider in colliders) {
                if (collider.gameObject.name == previousRoad.name) {previousRoadContactPoint = pointObject; break;}
                if (collider.gameObject.name == nextRoad.name) {nextRoadContactPoint = pointObject; break;}
            }
        }

        float delta_x = nextRoadContactPoint.x - previousRoadContactPoint.x;
        float delta_z = nextRoadContactPoint.z - previousRoadContactPoint.z;
        float angle = Mathf.Atan2(delta_z, delta_x) * Mathf.Rad2Deg;

        //Debug.Log(angle);
        float tollerance = 2.5f;
        float point_tolerance = 0.05f;
        Direction direction = Direction.NONE;
        int contactPointIndex = -1;

        if ((previousRoadContactPoint.x <= contactPoint[3].x + point_tolerance && previousRoadContactPoint.x >= contactPoint[3].x - point_tolerance)  &&
            (previousRoadContactPoint.z <= contactPoint[3].z + point_tolerance && previousRoadContactPoint.z >= contactPoint[3].z - point_tolerance)) {
            //Debug.Log("0");
            contactPointIndex = 3;
            if (angle > -45 - tollerance && angle < -45 + tollerance) {
                direction = Direction.LEFT;
            }

            if (angle > -135 - tollerance && angle < -135 + tollerance) {
                direction = Direction.RIGHT;
            }

            if (angle > -90 - tollerance && angle < -90 + tollerance) {
                direction = Direction.STRAIGHT;
            }
        }
        
        
        if ((previousRoadContactPoint.x <= contactPoint[1].x + point_tolerance && previousRoadContactPoint.x >= contactPoint[1].x - point_tolerance)  &&
            (previousRoadContactPoint.z <= contactPoint[1].z + point_tolerance && previousRoadContactPoint.z >= contactPoint[1].z - point_tolerance)) {
            //Debug.Log("1");
            contactPointIndex = 1;
            if (angle > 135 - tollerance && angle < 135 + tollerance) {
                direction = Direction.LEFT;
            }

            if (angle > 45 - tollerance && angle < 45 + tollerance) {
                direction = Direction.RIGHT;
            }

            if (angle > 90 - tollerance && angle < 90 + tollerance) {
                direction = Direction.STRAIGHT;
            }
        }

        if ((previousRoadContactPoint.x <= contactPoint[2].x + point_tolerance && previousRoadContactPoint.x >= contactPoint[2].x - point_tolerance)  &&
            (previousRoadContactPoint.z <= contactPoint[2].z + point_tolerance && previousRoadContactPoint.z >= contactPoint[2].z - point_tolerance)) {
            //Debug.Log("2");
            contactPointIndex = 2;
            if (angle > 45 - tollerance && angle < 45 + tollerance) {
                direction = Direction.LEFT;
            }

            if (angle > -45 - tollerance && angle < -45 + tollerance) {
                direction = Direction.RIGHT;
            }

            if (angle > 0 - tollerance && angle < 0 + tollerance) {
                direction = Direction.STRAIGHT;
            }else if (angle > 180 - tollerance && angle < 180 + tollerance) {
                direction = Direction.STRAIGHT;
            }
        }

        if ((previousRoadContactPoint.x <= contactPoint[0].x + point_tolerance && previousRoadContactPoint.x >= contactPoint[0].x - point_tolerance)  &&
            (previousRoadContactPoint.z <= contactPoint[0].z + point_tolerance && previousRoadContactPoint.z >= contactPoint[0].z - point_tolerance)) {
            //Debug.Log("3");
            contactPointIndex = 0;
            if (angle > -135 - tollerance && angle < -135 + tollerance) {
                direction = Direction.LEFT;
            }

            if (angle > 135 - tollerance && angle < 135 + tollerance) {
                direction = Direction.RIGHT;
            }

            if (angle > 180 - tollerance && angle < 180 + tollerance) {
                direction = Direction.STRAIGHT;
            }else if (angle > 0 - tollerance && angle < 0 + tollerance) {
                direction = Direction.STRAIGHT;
            }
        }

        return (contactPointIndex, direction);
    }
    private (int, Direction) GetThreeWayDirection(string previous, string next) {
        GameObject previousRoad = GameObject.Find(previous);
        GameObject nextRoad = GameObject.Find(next);

        float minDistancePrevious = Mathf.Infinity;
        int indexMinPrevious = -1; 

        float minDistanceNext = Mathf.Infinity;
        int indexMinNext = -1; 

        bool isFoundPrevious = false;
        bool isFoundNext = false;

        for (int i = 0; i < this.waypoints.Length; i++) {
            if (waypoints[i].tag == "intersectionEnter") {
                float distance = Vector3.Distance(waypoints[i].transform.position, previousRoad.transform.position);
                if (distance < minDistancePrevious) {
                    minDistancePrevious = distance;
                    indexMinPrevious = i;
                    isFoundPrevious = true;
                }
            }

            if (waypoints[i].tag == "intersectionOut") {
                float distance = Vector3.Distance(waypoints[i].transform.position, nextRoad.transform.position);
                if (distance < minDistanceNext) {
                    minDistanceNext = distance;
                    indexMinNext = i;
                    isFoundNext = true;
                }
            }

            if (isFoundPrevious && isFoundNext) {break;}
        }

        if (indexMinNext == -1) {
            Debug.LogError("indexMinNext: " + indexMinNext);
            return (0, Direction.NONE);
        }

        if (indexMinPrevious == 0) {
            if (indexMinNext == 7) {return (1, Direction.LEFT);}
            if (indexMinNext == 5) {return (1, Direction.RIGHT);}
        }

        if (indexMinPrevious == 4) {
            if (indexMinNext == 5) {return (2, Direction.STRAIGHT);}
            if (indexMinNext == 1) {return (2, Direction.RIGHT);}
        }

        if (indexMinPrevious == 6) {
            if (indexMinNext == 7) {return (0, Direction.LEFT);}
            if (indexMinNext == 5) {return (0, Direction.RIGHT);}
        }

        return (0, Direction.NONE);
    }
    private Vector3[] FourWayIntersection(Vector3 previousRoadContactPoint, Direction direction, float spunkRate) {
        (Vector3 enterPoint, int index) = GetEnterPoint(previousRoadContactPoint);

        if (direction == Direction.STRAIGHT) {return GetStraightPath(enterPoint, index);}

        if (direction == Direction.LEFT) {return GetLeftPath(enterPoint, index, spunkRate);}

        return GetRightPath(enterPoint, index, spunkRate);
    }

    private Vector3[] ThreeWayIntersection(Vector3 previousRoadContactPoint, Direction direction, float spunkRate) {
        (Vector3 enterPoint, int index) = GetEnterPoint(previousRoadContactPoint);
        Debug.Log("index: " + index);

        if (direction == Direction.STRAIGHT) {return GetStraightPath_ThreeWay(enterPoint, index);}

        if (direction == Direction.LEFT) {return GetLeftPath_ThreeWay(enterPoint, index, spunkRate);}

        return GetRightPath_ThreeWay(enterPoint, index, spunkRate);
    }

    private Vector3[] Roundabout(Vector3 previousRoadContactPoint, Direction direction, float spunkRate) {
        (Vector3 enterPoint, int index) = GetEnterPoint(previousRoadContactPoint);

        if (direction == Direction.STRAIGHT) {return GetSpinPath(enterPoint, index, spunkRate, 0.25f);}

        if (direction == Direction.LEFT) {return GetSpinPath(enterPoint, index, spunkRate, 0.5f);}

        return GetSpinPath(enterPoint, index, spunkRate, 0.75f);
    }

    private void UpdatePath() {
        var lastTuple = path[path.Count-1];
        path.RemoveAt(path.Count-1);

        foreach (Vector3 point in this.tempPath) {
            path.Add((point, currentRoadName, currentRoadType));
        }
        
        path.Add(lastTuple);
    }

    private (Vector3, int) GetEnterPoint(Vector3 enterPoint) {
        float minDistance = Mathf.Infinity;
        int index = 0;
        int i = 0;

        foreach (GameObject waypoint in waypoints) {
            if (waypoint.tag == "intersectionEnter") {
               if (Vector3.Distance(enterPoint, waypoint.transform.position) < minDistance) {
                    minDistance = Vector3.Distance(enterPoint, waypoint.transform.position);
                    index = i;
               }
            }
            i++;
        }

        return (waypoints[index].transform.position, index);
    }
        private Vector3[] GetStraightPath(Vector3 enterPoint, int index) {
        
                        
        if (Vector3.Distance(enterPoint, waypoints[0].transform.position) < 0.2f) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[14].transform.position;
            path[2] = waypoints[8].transform.position;
            path[3] = waypoints[2].transform.position;
            Debug.Log("0-14-8-2");
            return path;

        }else if (Vector3.Distance(enterPoint, waypoints[3].transform.position) < 0.2f) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[9].transform.position;
            path[2] = waypoints[11].transform.position;
            path[3] = waypoints[1].transform.position;
            Debug.Log("3-9-11-1");
            return path;

        }else if (Vector3.Distance(enterPoint, waypoints[4].transform.position) < 0.2f) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[11].transform.position;
            path[2] = waypoints[14].transform.position;
            path[3] = waypoints[5].transform.position;
            Debug.Log("4-11-14-5");
            return path;

        }else if (Vector3.Distance(enterPoint, waypoints[6].transform.position) < 0.2f) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[8].transform.position;
            path[2] = waypoints[9].transform.position;
            path[3] = waypoints[7].transform.position;
            Debug.Log("6-8-9-7");
            return path;

        }else {
            Vector3[] path = new Vector3[0];
            Debug.Log("path null");
            return path;
        }  
    }
    private Vector3[] GetStraightPath_ThreeWay(Vector3 enterPoint, int index) {
        
                        
        if (index == 4) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[11].transform.position;
            path[2] = waypoints[2].transform.position;
            path[3] = waypoints[5].transform.position;
            Debug.Log("4-11-2-5");
            return path;

        }else if (index == 6) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[8].transform.position;
            path[2] = waypoints[9].transform.position;
            path[3] = waypoints[7].transform.position;
            Debug.Log("6-8-9-7");
            return path;

        }else {
            Vector3[] path = new Vector3[0];
            Debug.Log("path null");
            return path;
        }  
    }
    private Vector3[] GetLeftPath(Vector3 enterPoint, int index, float spunkRate) {
        
                        
        if (index == 0) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[14].transform.position;
            path[2] = waypoints[12].transform.position;
            path[3] = waypoints[9].transform.position;
            path[4] = waypoints[7].transform.position;
            Debug.Log("0-14-12-9-7");
            return path;

        }else if (index == 3) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[9].transform.position;
            path[2] = waypoints[15].transform.position;
            path[3] = waypoints[14].transform.position;
            path[4] = waypoints[5].transform.position;
            Debug.Log("3-9-15-14-5");
            return path;


        }else if (index == 6) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[8].transform.position;
            path[2] = waypoints[13].transform.position;
            path[3] = waypoints[11].transform.position;
            path[4] = waypoints[1].transform.position;
            Debug.Log("6-8-13-11-1");
            return path;


        }else if (index == 4) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[11].transform.position;
            path[2] = waypoints[10].transform.position;
            path[3] = waypoints[8].transform.position;
            path[4] = waypoints[2].transform.position;
            Debug.Log("4-11-10-8-2");     
            return path;
        }else {
            Vector3[] path = new Vector3[0];
            Debug.Log("path null");
            return path;
        }
    }
    private Vector3[] GetLeftPath_ThreeWay(Vector3 enterPoint, int index, float spunkRate) {
        
                        
        if (index == 0) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[2].transform.position;
            path[2] = waypoints[10].transform.position;
            path[3] = waypoints[9].transform.position;
            path[4] = waypoints[7].transform.position;
            Debug.Log("0-2-10-9-7");
            return path;

        }else if (index == 6) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[8].transform.position;
            path[2] = waypoints[3].transform.position;
            path[3] = waypoints[11].transform.position;
            path[4] = waypoints[1].transform.position;
            Debug.Log("6-8-3-11-1");
            return path;


        }else {
            Vector3[] path = new Vector3[0];
            Debug.Log("path null");
            return path;
        }
    }
    private Vector3[] GetRightPath(Vector3 enterPoint, int index, float spunkRate) {
            
        if (index == 0) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[14].transform.position;
            path[2] = waypoints[5].transform.position;
            Debug.Log("0-14-5");
            return path;

        }else if (index == 3) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[9].transform.position;
            path[2] = waypoints[7].transform.position;
            Debug.Log("3-9-7");
            return path;

        }else if (index == 6) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[8].transform.position;
            path[2] = waypoints[2].transform.position;
            Debug.Log("6-8-2");
            return path;

        }else if (index == 4) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[11].transform.position;
            path[2] = waypoints[1].transform.position;
            Debug.Log("11-1-0");
            return path;

        }else {
            Vector3[] path = new Vector3[0];
            Debug.Log("path null");
            return path;
        }   
    }
    private Vector3[] GetRightPath_ThreeWay(Vector3 enterPoint, int index, float spunkRate) {
            
        if (index == 0) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[2].transform.position;
            path[2] = waypoints[5].transform.position;
            Debug.Log("0-14-5");
            return path;

        }else if (index == 4) {
            Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
            path[1] = waypoints[11].transform.position;
            path[2] = waypoints[1].transform.position;
            Debug.Log("11-1-0");
            return path;

        }else {
            Vector3[] path = new Vector3[0];
            Debug.Log("path null");
            return path;
        }   
    }
    

    private void ClearPath() {
        for (int i = 0; i < tempPath.Length; i++) {
            tempPath[i] = new Vector3(0, 0, 0);
        }
    }
    
    public bool IsNormalized (int lastPathLength) {
        Vector3 destination = path[path.Count-1].Item1;
        Vector3 enterPoint = path[path.Count-2-additionalPathLength].Item1;

        Debug.Log("lastPathLength: " + lastPathLength);
        Debug.Log("path.Count: " + path.Count);

        if (additionalPathLength == -1 || lastPathLength == path.Count) {
            return false;
        }

        Vector3 minDistancePointDestination = new Vector3(0, 0, 0);
        Vector3 minDistancePointEnter = new Vector3(0, 0, 0);
        float minDistanceDestination = Mathf.Infinity;
        float minDistanceEnter = Mathf.Infinity;

        for (int i = lastPathLength-1; i < path.Count-1; i++) {
            if (Vector3.Distance(destination, path[i].Item1) < minDistanceDestination) {
                minDistanceDestination = Vector3.Distance(destination, path[i].Item1);
                minDistancePointDestination = path[i].Item1;
            }
        }

        for (int i = lastPathLength-1; i < path.Count-1; i++) {
            if (Vector3.Distance(enterPoint, path[i].Item1) < minDistanceEnter) {
                minDistanceEnter = Vector3.Distance(enterPoint, path[i].Item1);
                minDistancePointEnter = path[i].Item1;
            }
        }

        if (Vector3.Distance(path[path.Count-2].Item1, minDistancePointDestination) < 0.09f && Vector3.Distance(path[lastPathLength-1].Item1, minDistancePointEnter) < 0.09f) {
            return true;
        }
        return false;
    }
    // TODO: implementare la rotonda
    private Vector3[] GetSpinPath(Vector3 enterPoint, int index, float spunkRate, float angle) {return new Vector3[0];}

}
