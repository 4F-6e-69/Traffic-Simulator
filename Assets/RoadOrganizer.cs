using UnityEngine;
using System.Collections.Generic;
using System;

public class RoadOrganizer : MonoBehaviour
{
    [SerializeField] int index = 0;
    [SerializeField] GameObject start;

    [ContextMenu("Organize")]
    public void Organize() {
        CarPath carPath = gameObject.GetComponent<CarPath>();
        List<GameObject> percorso = carPath.GetNodes(8);
        List<GameObject> roads = new List<GameObject>();
        for (int i = percorso.Count - 1; i >= 0; i--) {
            roads.Add(percorso[i]);
        }

        for (int i = 0; i < roads.Count; i++) {
            carPath.AddPath(roads[i], 7);
        }
        /*
        Vector3[] points = new Vector3[count];

        for (int i = 0; i < count; i++) {
            points[i] = roads[i].transform.position;
            roads[i].gameObject.tag = "Untagged";
        }

        Array.Sort(points, (a, b) => Vector3.Distance(a, points[0]).CompareTo(Vector3.Distance(b, points[0])));

        for (int i = 0; i < count; i++) {
            carPath.AddPath(roads[i], index);
        }
        */
    }

}