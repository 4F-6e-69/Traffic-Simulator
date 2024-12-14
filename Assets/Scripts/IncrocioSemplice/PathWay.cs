using UnityEngine;
using System;
using System.Collections.Generic;

public class PathWay
{
    public class Node {
        private string roadame;
        public string RoadName { 
            get{
                return roadame;
            }

            set{
                roadame = value;
            }
        }

        private string roadType;
        public string RoadType { 
            get{
                return roadType;
            }

            set{
                roadType = value;
            }
        }

        private Vector3 position;
        public Vector3 Position { 
            get{
                return position;
            }

            set{
                position = value;
            }
        }

        private int index;
        public int Index { 
            get{
                return index;
            }

            set{
                index = value;
            }
        }
        
        private Node nextNode;  
        public Node NextNode { 
            get{
                return nextNode;
            }

            set{
                nextNode = value;
            }
        }

        private Node previousNode;
        public Node PreviousNode { 
            get{
                return previousNode;
            }

            set{
                previousNode = value;
            }
        }

        public Node(Node nextNode, Node previousNode, string roadame, string roadType, Vector3 position, int index) {

            this.roadame = roadame;
            this.roadType = roadType;
            this.position = position;

            this.nextNode = nextNode;
            this.previousNode = previousNode;
            this.Index = index;
        }

    }
    
    private Node head = null;
    private Node tail = null;
    private int count = 0;
    public int Count { 
        get{
            return count;
        }
    }

    private List<Vector3> nodes = new List<Vector3>();
    public Vector3 Nodes (int index) {
        return nodes[index];
    }
    public PathWay(string SpawnRoadame, string SpawnRoadType, Vector3 SpawnPosition, string EndRoadame, string EndRoadType, Vector3 EndPosition) {
        count = 0;

        head = new Node(null, this.tail, SpawnRoadame, SpawnRoadType, SpawnPosition, count);
        tail = new Node(this.head, null, EndRoadame, EndRoadType, EndPosition, ++count);
        nodes.Add(SpawnPosition);
        nodes.Add(EndPosition);
    }
/*
    public string SpawnName {
        get{
            return head.RoadName;
        }
    }
    public Vector3 SpawnPosition {
        get{
            return head.Position;
        }
    }   

    public string EndName {
        get{
            return tail.RoadName;
        }
    }
    public Vector3 EndPosition {
        get{
            return tail.Position;
        }
    }

    public void AddNode(string roadame, string roadType, Vector3 position) {
        Node tempTail = new Node(tail.PreviousNode, null, roadame, roadType, position, count-2);

        this.tail = new Node(this.tail.PreviousNode, tempTail, roadame, roadType, position, count-2);
        tempTail.PreviousNode.NextNode = this.tail;
        tempTail.PreviousNode = this.tail;

        tempTail.Index = count;
        this.tail = tempTail;
        count++;
        nodes.RemoveAt(count-1);
        nodes.Add(position);
        nodes.Add(tempTail.Position);
    }

    public bool AddNodeRange(Vector3[] positions, string[] names, string[] types) {
        Vector3 tempNode = this.tail.Position;

        if (CheckRange(positions, names, types)) {

            foreach (Vector3 position in positions) {
                AddNode(names[Array.IndexOf(positions, position)], types[Array.IndexOf(positions, position)], position);
                
            }
            
            nodes.RemoveAt(count-1);
            nodes.AddRange(positions);
            nodes.Add(tempNode);
            return true;
        }
        return false;
    }
    
    public Node RemovePoint(int index) {
        nodes.RemoveAt(index);
        int middle = Mathf.CeilToInt(count / 2);

        Node tempPrevNode = null;
        Node tempNextNode = null;
        Node nodeToRemove = this.tail;
        
        if (index >= middle) {

            for (int i = count-1; i > index; i--) {
                nodeToRemove = nodeToRemove.PreviousNode;
            }
        }

        if (index < middle) {

            for (int i = 0; i < index-1; i++) {
                nodeToRemove = nodeToRemove.NextNode;
            }
        }
    
        tempNextNode = nodeToRemove.NextNode;
        tempPrevNode = nodeToRemove.PreviousNode;

        tempPrevNode.NextNode = tempNextNode;
        tempNextNode.PreviousNode = tempPrevNode;

        while (tempNextNode != null) {
            tempNextNode.Index--;
            tempNextNode = tempNextNode.NextNode;
        }

        count--;
        return nodeToRemove;
    }

    public Node RemoveLast() {
        nodes.RemoveAt(count-1);
        return RemovePoint(Count-1);

    }

    public Vector3 getPosition(int index) {
        return nodes[index];
    }

    public override string ToString() {
        return "Head: " + head.RoadName + " Tail: " + tail.RoadName + " Count: " + Count;
    }

    private bool CheckRange (Vector3[] positions, string[] names, string[] types) {
        if (positions.Length != names.Length || positions.Length != types.Length) {
            return false;
        }

        return true;
    }

    public Vector3[] toArray() {
        return nodes.ToArray();
    }
*/
}