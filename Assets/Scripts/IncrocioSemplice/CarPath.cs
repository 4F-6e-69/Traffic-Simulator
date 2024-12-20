using UnityEngine;
using System.Collections.Generic;

public class CarPath : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> nodes_1 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_2 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_3 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_4 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_5 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_6 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_7 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_8 = new List<GameObject>();
    /*
    [SerializeField]
    private List<GameObject> nodes_9_0_1 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_9_0_2 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_9_2_0 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_9_2_1 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_9_1_0 = new List<GameObject>();
    [SerializeField]
    private List<GameObject> nodes_9_1_2 = new List<GameObject>();
    */

    private int pathCount = 8;
    public List<string> GetPath() {
        int index = Random.Range(1, pathCount+1);
        Debug.Log("index: " + index);

        switch (index) {
            case 1:
                return PathToString(nodes_1);
            case 2:
                return PathToString(nodes_2);
            case 3:
                return PathToString(nodes_3);
            case 4:
                return PathToString(nodes_4);*/
            case 5:
                return PathToString(nodes_5);/*
            case 6:
                return PathToString(nodes_6);
            case 7:
                return PathToString(nodes_7);
            case 8:
                return PathToString(nodes_8);
            
            default:
                return PathToString(nodes_5);
        }
    }

    public List<GameObject> GetNodes(int index) {
        switch (index) {
            case 1:
                return nodes_1;
            case 2:
                return nodes_2;
            case 3:
                return nodes_3;
            case 4:
                return nodes_4;
            case 5:
                return nodes_5;
            case 6:
                return nodes_6;
            case 7:
                return nodes_7;
            case 8:
                return nodes_8;
            default:
                return nodes_1;
        }
    }
    private List<string> PathToString(List<GameObject> nodes) {
            int count = nodes.Count;
            List<string> path = new List<string>();
            for (int i = 0; i < count; i++) {
                path.Add(nodes[i].name);
            }
            return path;
    }

    public void AddPath(GameObject node, int index) {
        switch (index) {
            case 1:
                nodes_1.Add(node);
                break;
            case 2:
                nodes_2.Add(node);
                break;
            case 3:
                nodes_3.Add(node);
                break;
            case 4:
                nodes_4.Add(node);
                break;
            case 5:
                nodes_5.Add(node);
                break;
            case 6:
                nodes_6.Add(node);
                break;
            case 7:
                nodes_7.Add(node);
                break;
            case 8:
                nodes_8.Add(node);
                break;
            default:
                break;
        }
    }
}
