using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class NodeGroup : SerializedMonoBehaviour
{
    static readonly List<NodeGroup> allNodeGroups = new();

    public List<Node> nodes = new();
    public List<NodeGroup> neighbors = new();
    public List<Ship> ships = new();
    public List<ManagedInterest> managedInterests = new();

    [SerializeField]
    Node startingNode;

    public bool ContainsPlayer = false;
    public bool InInterest = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool LastInterest = true;
    // Update is called once per frame
    void Update()
    {


    }

    //Needs to be late so that every group has a chance to see if it contains the player
    //Classic variation of the "off by one" error
    private void LateUpdate()
    {
        InInterest = false;
        if (ContainsPlayer)
            InInterest = true;
        else
        {
            foreach (NodeGroup ng in neighbors)
            {
                if (ng.ContainsPlayer)
                {
                    InInterest = true;
                }
            }
        }


        if (InInterest != LastInterest)
        {
            if (InInterest)
                ComeIntoInterest();
            else
                LeaveInterest();

        }
        LastInterest = InInterest;
    }

    public void ComeIntoInterest()
    {
        foreach (ManagedInterest mi in managedInterests)
        {
            mi.gameObject.SetActive(true);
        }

        foreach (Ship s in ships)
        {
            s.gameObject.SetActive(true);
            if (s.enemyManager)
            {
                s.enemyManager.EnterInterest();
            }
                
        }
    }

    public void LeaveInterest()
    {
        foreach (ManagedInterest mi in managedInterests)
        {
            mi.gameObject.SetActive(false);
        }
        foreach (Ship s in ships)
        {
            if (s.ai)
            {
                if (s.ai.aggro)
                    break; //Don't move aggroed enemies. This is a failsafe
            }

            s.gameObject.SetActive(false);
            if (s.enemyManager)
            {
                s.enemyManager.LeaveInterest();
            }
        }
    }




    [Button]
    public void Precalculate()
    {
        Precalculate(true);
    }

    public void Precalculate(bool chain)
    {
#if UNITY_EDITOR
        if (chain)
        {
            Node randomNode = FindObjectOfType<Node>();
            randomNode.Precalculate(false);
        }


        allNodeGroups.Clear();
        allNodeGroups.AddRange(FindObjectsOfType<NodeGroup>());

        foreach (NodeGroup ng in allNodeGroups)
        {
            ng.nodes.Clear();
            ng.startingNode = Node.FindNearestNode(ng.transform.position);
        }

        foreach (Node n in Node.allNodes)
        {
            NodeGroup bestSoFar = null;
            float distance = Mathf.Infinity;
            foreach (NodeGroup ng in allNodeGroups)
            {
                float thisDist = ng.startingNode.CalculatePathDistance(n);
                if (thisDist < distance)
                {
                    distance = thisDist;
                    bestSoFar = ng;
                }
            }
            bestSoFar.AssignNodeGroup(n);
        }

        foreach (NodeGroup ng in allNodeGroups)
        {
            ng.neighbors.Clear();
            foreach (NodeGroup nodeGroup in allNodeGroups)
            {
                foreach (Node n in nodeGroup.nodes)
                {
                    foreach (Node neighbor in n.adjacentNodes.Keys)
                    {
                        if (ng.nodes.Contains(neighbor))
                        {
                            ng.neighbors.Add(nodeGroup);
                            break;
                        }
                    }
                    if (ng.neighbors.Contains(nodeGroup))
                        break;
                }
            }

            ng.ships.Clear();
            ng.managedInterests.Clear();
            foreach (Ship s in FindObjectsByType<Ship>(FindObjectsSortMode.None))
            {
                if (ng.nodes.Contains(s.startingNode))
                {
                    ng.ships.Add(s);
                }
            }
            foreach (ManagedInterest mi in FindObjectsByType<ManagedInterest>(FindObjectsSortMode.None))
            {
                if (ng.nodes.Contains(mi.defaultNode))
                {
                    ng.managedInterests.Add(mi);
                }
            }

            UnityEditor.EditorUtility.SetDirty(ng.gameObject);
            UnityEditor.EditorUtility.SetDirty(ng);
        }

        Debug.Log("Done Precalculating!");
#endif
    }

    void AssignNodeGroup(Node n)
    {
        n.myNodeGroup = this;
        nodes.Add(n);
    }
}
