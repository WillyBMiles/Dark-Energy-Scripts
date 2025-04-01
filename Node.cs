using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class Node : SerializedMonoBehaviour
{
    public static List<Node> allNodes = new();

    public Dictionary<Node, float> adjacentNodes = new();
    public Dictionary<Node, Node> cameFrom = new();

    public NodeGroup myNodeGroup;

    private void Awake()
    {
        allNodes.Add(this);
    }
    private void OnDestroy()
    {
        allNodes.Remove(this);
    }

    [Button]
    public void Precalculate()
    {
        Precalculate(true);
    }

    public void Precalculate(bool chain)
    {
#if UNITY_EDITOR
        allNodes.Clear();
        allNodes.AddRange(FindObjectsByType<Node>(FindObjectsSortMode.InstanceID));
        foreach (Node n in allNodes)
        {
            n.FindAdjacent();
        }


        foreach (Node n in allNodes)
        {
            n.FindAllPaths();
            UnityEditor.EditorUtility.SetDirty(n.gameObject);
            UnityEditor.EditorUtility.SetDirty(n);
        }
        
        if (allNodes.Count > cameFrom.Count)
        {
            Debug.LogWarning("Node graph is unconnected!");

            int minimum = int.MaxValue;
            Node minNode = null;
            foreach (Node n in allNodes)
            {
                if (n.cameFrom.Count < minimum)
                {
                    minimum = n.cameFrom.Count;
                    minNode = n;
                }
                    
            }
            Debug.LogWarning($"Least connected node: {minNode.name}");
        }




        foreach (Ship s in FindObjectsByType<Ship>(FindObjectsSortMode.None))
        {
            s.Precalculate();
            UnityEditor.EditorUtility.SetDirty(s.gameObject);
            UnityEditor.EditorUtility.SetDirty(s);
        }

        foreach (ManagedInterest mi in FindObjectsByType<ManagedInterest>(FindObjectsSortMode.None))
        {
            mi.Precalculate();
            UnityEditor.EditorUtility.SetDirty(mi.gameObject);
            UnityEditor.EditorUtility.SetDirty(mi);
        }


        if (chain)
        {
            NodeGroup nodeGroup = FindObjectOfType<NodeGroup>();
            nodeGroup.Precalculate(false);
        }
        
#endif
    }

    void FindAdjacent()
    {
        
        adjacentNodes.Clear();
        adjacentNodes[this] = 0f;
        foreach (Node n in FindObjectsByType<Node>(FindObjectsSortMode.InstanceID))
        {
            if (n != this)
            {
                if (IsInLOS(n.transform.position))
                {
                    adjacentNodes[n] = Vector2.Distance(transform.position, n.transform.position);
                }
            }
        }
    }

    void FindAllPaths()
    {
        cameFrom.Clear();

        List<Node> testNodes = new() { this };
        List<Node> settledNodes = new();
        while (testNodes.Count > 0)
        {
            testNodes.Sort((n1, n2) => CalculatePathDistance(n1).CompareTo(CalculatePathDistance(n2)));
            PathIteration(testNodes[0], testNodes, settledNodes);
            settledNodes.Add(testNodes[0]);
            testNodes.RemoveAt(0);
            
        }
    }

    void PathIteration( Node currentNode, List<Node> testSet, List<Node> settledNodes)
    {
        List<Node> adjacentNodes = new();
        foreach (Node n in currentNode.adjacentNodes.Keys)
        {
            adjacentNodes.Add(n);
        }
        adjacentNodes.Sort((Node v1, Node v2) => currentNode.adjacentNodes[v1].CompareTo(currentNode.adjacentNodes[v2]));

        // Go through every adjacent node and update it's node distances
        foreach (Node n in adjacentNodes)
        {
            if (!cameFrom.ContainsKey(n) || CalculatePathDistance(n) > CalculatePathDistance(n, currentNode))
            {
                cameFrom[n] = currentNode;
            }
            if (!testSet.Contains(n) && !settledNodes.Contains(n))
            {
                testSet.Add(n);
            }
        }

    }
    public float CalculatePathDistance(Node n, Node overrideLast = null)
    {
        if (n == this)
            return 0f;
        Node last = overrideLast != null ? overrideLast : cameFrom[n];
        return last.adjacentNodes[n] + CalculatePathDistance(last);

    }
    Node GetNextNode(Node destination, Vector2 origin)
    {
        if (destination == this || IsInLOS(origin, destination.transform.position))
            return destination;
        return GetNextNode(cameFrom[destination], origin);
    }

    public static Node FindNearestNode(Vector2 point)
    {
        float minDist = float.PositiveInfinity;
        Node nearest = null;
        foreach (Node n in allNodes)
        {
            float dist = Vector2.Distance(point, n.transform.position);
            if (n.IsInLOS(point) &&  dist< minDist)
            {
                minDist = dist;
                nearest = n;
            }
        }
        if (nearest == null)
        {
            Debug.LogWarning($"Can't see location: {point} from Nodes!");
            foreach (Node n in allNodes)
            {
                float dist = Vector2.Distance(point, n.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = n;
                }
            }
        }
        return nearest;
    }

    public static Node FindNearestNode(Vector2 point, Node starting)
    {
        float minDist = float.PositiveInfinity;
        Node nearest = null;
        foreach (Node n in starting.adjacentNodes.Keys)
        {
            float dist = Vector2.Distance(point, n.transform.position);
            if (n.IsInLOS(point) && dist < minDist)
            {
                minDist = dist;
                nearest = n;
            }
        }

        if (nearest == null) //If adjacent nodes aren't nearest, use all nodes
        {
            nearest = FindNearestNode(point);
        }

        if (nearest == null)
        {
            Debug.LogWarning($"Can't see location: {point} from Nodes!");
            foreach (Node n in allNodes)
            {
                float dist = Vector2.Distance(point, n.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = n;
                }
            }
        }
        return nearest;
    }

    public static Vector2 Pathfind(Vector2 origin, Node startNode,  Vector2 destination, Node endNode)
    {
        if (IsInLOS(destination, origin))
            return destination;
        return startNode.GetNextNode(endNode, origin).transform.position;
    }

    static LayerMask layerMask;
    bool IsInLOS(Vector2 point)
    {
        return IsInLOS(point, transform.position);
    }

    //static RaycastHit2D[] results = new RaycastHit2D[1];
    //static ContactFilter2D contactFilter2D = new ContactFilter2D() { layerMask = 1<<9, useLayerMask = true };
    public static bool IsInLOS(Vector2 point, Vector2 origin)
    {
        layerMask = LayerMask.GetMask("Environment");
#if UNITY_EDITOR

        //Physics2D.Raycast(origin, (Vector2)point - (Vector2)origin, contactFilter2D, results, Vector2.Distance(origin, point));

#endif

        return
    !Physics2D.Raycast(origin, (Vector2)point - (Vector2)origin, Vector2.Distance(origin, point), layerMask);
    }


}
