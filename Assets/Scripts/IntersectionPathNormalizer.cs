using UnityEngine;
using System;
using System.Collections.Generic;

public class IntersectionPathNormalizer : MonoBehaviour
{
    [SerializeField] private Vector3 boxSize = new Vector3(2.5f, 3f, 2.5f);
    [SerializeField] private GameObject[] waypoints;
    [SerializeField] private GameObject center;
    private Collider[] oldColliders = new Collider[0];

    private List<(Vector3, Vector3, string)> path = new List<(Vector3, Vector3, string)>();
 
    private void OnEnter(Collider other) {
        Vector3 nullVector = new Vector3(0, 100, 0);
        string[] colliderTag = other.gameObject.name.Split('_');

        if (colliderTag.Length < 2 || colliderTag.Length > 2) {return;}
        if (colliderTag[0] == "p" || colliderTag[0] == "car") {return;}
        if (Mathf.Abs(center.transform.position.x - other.gameObject.transform.position.x) > 2.5f || Mathf.Abs(center.transform.position.z - other.gameObject.transform.position.z) > 2.5f) {return;}
        
        string type = colliderTag[1].Split('-')[0];
        string colliderName = colliderTag[0];

        if (GameObject.Find(colliderName).GetComponent<CarController>().isDestroyed) {return;}
    
        if (type == "PathAI") {
            int index = path.FindIndex(x => x.Item3 == colliderName);
            //Debug.Log(index);

            
            if (index != -1) {

                var existingTuple = path[index];
                Debug.Log(existingTuple.Item1 + " " + existingTuple.Item2 + " " + existingTuple.Item3);
                if (existingTuple.Item2 == nullVector) {
                    existingTuple.Item2 = other.gameObject.transform.position;
                    //Debug.Log("aggiunto punto_2");
                }else {
                    existingTuple.Item1 = other.gameObject.transform.position;
                    //Debug.Log("aggiunto punto_1");
                }

                path[index] = existingTuple;
                Debug.Log(existingTuple.Item1 + " " + existingTuple.Item2 + " " + existingTuple.Item3);
            }else {
                path.Add((other.gameObject.transform.position, nullVector, colliderName));
                //Debug.Log("aggiunta path");
            }
            
        }

        //Debug.Log("fine primo ciclo");
    }
    
    private void UpdatePath() {
        Collider[] colliders = Physics.OverlapBox(center.transform.position, boxSize);
        if (colliders.Length != oldColliders.Length) {
            foreach (Collider collider in colliders) {
                OnEnter(collider);
                oldColliders = colliders;
            }
        }
    }

    public void RemovePath(string name) {
        path.RemoveAll(x => x.Item3 == name);
    }

    public Vector3[] Normalize (Vector3 start) {
        float minDistance = float.MaxValue;
        Vector3 closestPoint = Vector3.zero;

        foreach (var waypoint in waypoints) {
            if (waypoint.gameObject.tag == "intersectionEnter") {
                float distance = Vector3.Distance(start, waypoint.transform.position);
                if (distance < minDistance) {
                    minDistance = distance;
                    closestPoint = waypoint.transform.position;
                }
            }
        }

        Vector3[] path = new Vector3[] {closestPoint, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)};

        if (closestPoint == waypoints[0].transform.position) {
            path[1] = waypoints[14].transform.position;
            path[2] = waypoints[8].transform.position;
            path[3] = waypoints[2].transform.position;
            Debug.Log("0-14-8-2");
        }else if (closestPoint == waypoints[3].transform.position) {
            path[1] = waypoints[9].transform.position;
            path[2] = waypoints[11].transform.position;
            path[3] = waypoints[1].transform.position;
            Debug.Log("3-9-11-1");
        }else if (closestPoint == waypoints[6].transform.position) {
            path[1] = waypoints[8].transform.position;
            path[2] = waypoints[9].transform.position;
            path[3] = waypoints[7].transform.position;
            Debug.Log("6-8-9-7");
        }else {
            path[1] = waypoints[11].transform.position;
            path[2] = waypoints[14].transform.position;
            path[3] = waypoints[5].transform.position;
            Debug.Log("11-14-5-0");
        }   
        
        return path;
    }

    public Vector3[] Normalize(string name) {
        UpdatePath();

        Vector3 intersectionEnter = path.Find(x => x.Item3 == name).Item1;
        Vector3 intersectionExit = path.Find(x => x.Item3 == name).Item2;
        DirectionType direction = DirectionType.None;

        (direction, intersectionEnter, intersectionExit) = getDirection(intersectionExit, intersectionEnter);
        Debug.Log(direction);

        if (direction == DirectionType.Right) {
            Vector3[] path = new Vector3[] {intersectionEnter, new Vector3(0, 0, 0), intersectionExit};

            
            if (Vector3.Distance(intersectionEnter, waypoints[0].transform.position) < 0.2f) {
                path[1] = waypoints[14].transform.position;
                
            }else if (Vector3.Distance(intersectionEnter, waypoints[3].transform.position) < 0.2f) {
                path[1] = waypoints[9].transform.position;

            }else if (Vector3.Distance(intersectionEnter, waypoints[6].transform.position) < 0.2f) {
                path[1] = waypoints[8].transform.position;

            }else {
                path[1] = waypoints[11].transform.position;
            }
            
            return path;
        }
    
        if (direction == DirectionType.Left) {
            Vector3[] path = new Vector3[] {intersectionEnter, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), intersectionExit};
                        
            if (Vector3.Distance(intersectionEnter, waypoints[0].transform.position) < 0.2f) {
                path[1] = waypoints[14].transform.position;
                path[2] = waypoints[12].transform.position;
                path[3] = waypoints[9].transform.position;
                Debug.Log("0-14-12-9");

            }else if (Vector3.Distance(intersectionEnter, waypoints[3].transform.position) < 0.2f) {
                path[1] = waypoints[9].transform.position;
                path[2] = waypoints[15].transform.position;
                path[3] = waypoints[14].transform.position;
                Debug.Log("3-9-15-14");

            }else if (Vector3.Distance(intersectionEnter, waypoints[6].transform.position) < 0.2f) {
                path[1] = waypoints[8].transform.position;
                path[2] = waypoints[13].transform.position;
                path[3] = waypoints[11].transform.position;
                Debug.Log("6-8-13-11");

            }else {
                path[1] = waypoints[11].transform.position;
                path[2] = waypoints[10].transform.position;
                path[3] = waypoints[8].transform.position;
                Debug.Log("11-10-8-0"); 
            }
            
            return path;
        }


        return new Vector3[3];
    }

    private (DirectionType, Vector3, Vector3) getDirection(Vector3 end, Vector3 start) {
        DirectionType direction = DirectionType.None;
        Vector3 intersectionEnter = new Vector3(0, 0, 0);
        Vector3 intersectionExit = new Vector3(0, 0, 0);
        
        
        if (center.transform.position.x - start.x < 0) {
            if (center.transform.position.z - start.z < 0) {
                //angolo in alto a destra
                if (Mathf.Abs(end.x) - Mathf.Abs(start.x) < 0) {
                    direction = DirectionType.Right;
                    intersectionEnter = waypoints[6].transform.position;
                    intersectionExit = waypoints[2].transform.position;
                }else {
                    direction = DirectionType.Left;
                    intersectionEnter = waypoints[3].transform.position;
                    intersectionExit = waypoints[5].transform.position;
                }
            }else {
                //angolo in basso a destra
                if (Mathf.Abs(end.x) - Mathf.Abs(start.x) < 0) {
                    direction = DirectionType.Left;
                    intersectionEnter = waypoints[6].transform.position;
                    intersectionExit = waypoints[1].transform.position;
                }else {
                    direction = DirectionType.Right;
                    intersectionEnter = waypoints[0].transform.position;
                    intersectionExit = waypoints[5].transform.position;
                }
            }
        }else {
            if (center.transform.position.z - start.z < 0) {
                //angolo in alto a sinistra
                if (Mathf.Abs(end.x) - Mathf.Abs(start.x) < 0) {
                    direction = DirectionType.Left;
                    intersectionEnter = waypoints[4].transform.position;
                    intersectionExit = waypoints[2].transform.position;
                }else {
                    direction = DirectionType.Right;
                    intersectionEnter = waypoints[3].transform.position;
                    intersectionExit = waypoints[7].transform.position;
                }
            }else {
                //angolo in basso a sinistra
                if (Mathf.Abs(end.x) - Mathf.Abs(start.x) < 0) {
                    direction = DirectionType.Right;
                    intersectionEnter = waypoints[4].transform.position;
                    intersectionExit = waypoints[1].transform.position;
                }else {
                    direction = DirectionType.Left;
                    intersectionEnter = waypoints[0].transform.position;
                    intersectionExit = waypoints[7].transform.position;
                }
            }
        }
        
        return (direction, intersectionEnter, intersectionExit);
    }
}

public enum DirectionType {
    None,
    Right, 
    Left
}

