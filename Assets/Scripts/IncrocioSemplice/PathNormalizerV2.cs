using UnityEngine;
using System.Collections.Generic;
public enum Direction {
    NONE,
    LEFT,
    RIGHT,
    STRAIGHT
}

public class PathNormalizerV2 : MonoBehaviour
{
    private GameObject[] waypoints;
    private Vector3[] contactPoint;
    private string currentRoadName;
    private string currentRoadType;
    private Vector3[] tempPath;
    private List<(Vector3, string, string)> path;

    public PathNormalizerV2(List<(Vector3, string, string)> path, string previous, float spunkRate) {
        this.path = path;
        Debug.Log(previous);
        NormalizeSingleRoad(previous, spunkRate);
    }

    public PathNormalizerV2 (List<(Vector3, string, string)> path, string previous, string current, string next, float spunkRate) {
        this.path = path;
        
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

        if (gameObject.tag == "RoundCourve") {this.tempPath = RoundeCurve(previous, spunkRate); UpdatePath();}
        if (gameObject.tag == "StraightRoad") {}
        if (gameObject.tag == "AngleCurve") {this.tempPath = AngleCurve(previous, spunkRate); UpdatePath();}

    }

    private Vector3[] RoundeCurve(string previous, float spunkRate) {
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

        if (index == 0) {
            newPath[0] = waypoints[0].transform.position;
            newPath[1] = waypoints[1].transform.position;
            newPath[2] = waypoints[2].transform.position;
            newPath[3] = waypoints[3].transform.position;

        }else {
            newPath[0] = waypoints[4].transform.position;
            newPath[1] = waypoints[5].transform.position;
            newPath[2] = waypoints[6].transform.position;
            newPath[3] = waypoints[7].transform.position;
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

        Vector3 [] newPath = new Vector3[3];

        if (index == 0) {
            newPath[0] = waypoints[0].transform.position;
            newPath[1] = waypoints[1].transform.position;
            newPath[2] = waypoints[2].transform.position;

        }else {
            newPath[0] = waypoints[3].transform.position;
            newPath[1] = waypoints[4].transform.position;
            newPath[2] = waypoints[5].transform.position;
        }

        return newPath;
    }

    private void NormalizeCrossRoad(string previous, string next, float spunkRate) {
        // normalizzazione su strada a più ingressi
        // --incroci a tre vie
        // --incroci a quattro vie
        // --rotonde

        
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

        if ((previousRoadContactPoint.x <= contactPoint[0].x + point_tolerance && previousRoadContactPoint.x >= contactPoint[0].x - point_tolerance)  &&
            (previousRoadContactPoint.z <= contactPoint[0].z + point_tolerance && previousRoadContactPoint.z >= contactPoint[0].z - point_tolerance)) {
            //Debug.Log("0");
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

        if ((previousRoadContactPoint.x <= contactPoint[3].x + point_tolerance && previousRoadContactPoint.x >= contactPoint[3].x - point_tolerance)  &&
            (previousRoadContactPoint.z <= contactPoint[3].z + point_tolerance && previousRoadContactPoint.z >= contactPoint[3].z - point_tolerance)) {
            //Debug.Log("3");
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

        if (direction == Direction.STRAIGHT) {
            Debug.Log("STRAIGHT");
        }else if (direction == Direction.LEFT) {
            Debug.Log("LEFT");
        }else if (direction == Direction.RIGHT) {
            Debug.Log("RIGHT");
        }

        Debug.Log(currentRoadType);

        if (currentRoadType == "3_way_intersection") {this.tempPath =  ThreeWayIntersection(previousRoadContactPoint, direction, spunkRate);}
        if (currentRoadType == "4_way_intersection") {this.tempPath =  FourWayIntersection(previousRoadContactPoint, direction, spunkRate);}
        if (currentRoadType == "roundabout") {this.tempPath =  Roundabout(previousRoadContactPoint, direction, spunkRate);}

        UpdatePath();
    }

    private Vector3[] FourWayIntersection(Vector3 previousRoadContactPoint, Direction direction, float spunkRate) {
        Vector3 enterPoint = GetEnterPoint(previousRoadContactPoint);

        if (direction == Direction.STRAIGHT) {return GetStraightPath(enterPoint);}

        if (direction == Direction.LEFT) {return GetLeftPath(enterPoint, spunkRate);}

        return GetRightPath(enterPoint, spunkRate);
    }

    private Vector3[] ThreeWayIntersection(Vector3 previousRoadContactPoint, Direction direction, float spunkRate) {
        Vector3 enterPoint = GetEnterPoint(previousRoadContactPoint);

        if (direction == Direction.STRAIGHT) {return GetStraightPath(enterPoint);}

        if (direction == Direction.LEFT) {return GetLeftPath(enterPoint, spunkRate);}

        return GetRightPath(enterPoint, spunkRate);
    }

    private Vector3[] Roundabout(Vector3 previousRoadContactPoint, Direction direction, float spunkRate) {
        Vector3 enterPoint = GetEnterPoint(previousRoadContactPoint);

        if (direction == Direction.STRAIGHT) {return GetSpinPath(enterPoint, spunkRate, 0.25f);}

        if (direction == Direction.LEFT) {return GetSpinPath(enterPoint, spunkRate, 0.5f);}

        return GetSpinPath(enterPoint, spunkRate, 0.75f);
    }

    private void UpdatePath() {
        var lastTuple = path[path.Count-1];
        path.RemoveAt(path.Count-1);

        foreach (Vector3 point in this.tempPath) {
            path.Add((point, currentRoadName, currentRoadType));
        }
        
        path.Add(lastTuple);
    }

    private Vector3 GetEnterPoint(Vector3 enterPoint) {
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

        return waypoints[index].transform.position;
    }
    private Vector3[] GetStraightPath(Vector3 enterPoint) {
        Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
                        
        if (Vector3.Distance(enterPoint, waypoints[0].transform.position) < 0.2f) {
            path[1] = waypoints[14].transform.position;
            path[2] = waypoints[8].transform.position;
            path[3] = waypoints[2].transform.position;
            Debug.Log("0-14-8-2");

        }else if (Vector3.Distance(enterPoint, waypoints[3].transform.position) < 0.2f) {
            path[1] = waypoints[9].transform.position;
            path[2] = waypoints[11].transform.position;
            path[3] = waypoints[1].transform.position;
            Debug.Log("3-9-11-1");

        }else if (Vector3.Distance(enterPoint, waypoints[4].transform.position) < 0.2f) {
            path[1] = waypoints[11].transform.position;
            path[2] = waypoints[14].transform.position;
            path[3] = waypoints[5].transform.position;
            Debug.Log("4-11-14-5");

        }else {
            path[1] = waypoints[11].transform.position;
            path[2] = waypoints[10].transform.position;
            path[3] = waypoints[8].transform.position;
            Debug.Log("4-11-10-8"); 
        }
            
        return path;
    }
    private Vector3[] GetLeftPath(Vector3 enterPoint, float spunkRate) {
        Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};
                        
        if (Vector3.Distance(enterPoint, waypoints[0].transform.position) < 0.2f) {
            path[1] = waypoints[14].transform.position;
            path[2] = waypoints[12].transform.position;
            path[3] = waypoints[9].transform.position;
            path[4] = waypoints[7].transform.position;
            Debug.Log("0-14-12-9-7");

        }else if (Vector3.Distance(enterPoint, waypoints[3].transform.position) < 0.2f) {
            path[1] = waypoints[9].transform.position;
            path[2] = waypoints[15].transform.position;
            path[3] = waypoints[14].transform.position;
            path[4] = waypoints[5].transform.position;
            Debug.Log("3-9-15-14-5");

        }else if (Vector3.Distance(enterPoint, waypoints[6].transform.position) < 0.2f) {
            path[1] = waypoints[8].transform.position;
            path[2] = waypoints[13].transform.position;
            path[3] = waypoints[11].transform.position;
            path[4] = waypoints[1].transform.position;
            Debug.Log("6-8-13-11-1");

        }else {
            path[1] = waypoints[11].transform.position;
            path[2] = waypoints[10].transform.position;
            path[3] = waypoints[8].transform.position;
            path[4] = waypoints[2].transform.position;
            Debug.Log("4-11-10-8-2"); 
        }
            
        return path;
    }
    private Vector3[] GetRightPath(Vector3 enterPoint, float spunkRate) {
        Vector3[] path = new Vector3[] {enterPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0)};

            
        if (Vector3.Distance(enterPoint, waypoints[0].transform.position) < 0.2f) {
            path[1] = waypoints[14].transform.position;
            path[2] = waypoints[5].transform.position;
            Debug.Log("0-14-5");
                
        }else if (Vector3.Distance(enterPoint, waypoints[3].transform.position) < 0.2f) {
            path[1] = waypoints[9].transform.position;
            path[2] = waypoints[7].transform.position;
            Debug.Log("3-9-7");

        }else if (Vector3.Distance(enterPoint, waypoints[6].transform.position) < 0.2f) {
            path[1] = waypoints[8].transform.position;
            path[2] = waypoints[2].transform.position;
            Debug.Log("6-8-2");

        }else {
            path[1] = waypoints[11].transform.position;
            path[2] = waypoints[1].transform.position;
            Debug.Log("11-1-0");
        }
            
        return path;
    }
    
    private void ClearPath() {
        for (int i = 0; i < tempPath.Length; i++) {
            tempPath[i] = new Vector3(0, 0, 0);
        }
    }
    
    
    // TODO: implementare la rotonda
    private Vector3[] GetSpinPath(Vector3 enterPoint, float spunkRate, float angle) {return new Vector3[0];}

}
