using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGroupManager : MonoBehaviour
{
    readonly List<NodeGroup> nodeGroups = new();

    // Start is called before the first frame update
    void Start()
    {
        nodeGroups.Clear();
        nodeGroups.AddRange(FindObjectsByType<NodeGroup>(FindObjectsSortMode.InstanceID));
    }

    List<NodeGroup> playerNodeGroups = new();
    // Update is called once per frame
    void Update()
    {

        /* This is the ONLY code that sets NodeGroup.ContainsPlayer
         Summary of code:
        Reset all node groups for containing player.
        Check each player to see which Node group they're in.

        If there are NO NodeGroups containing players, 
            Instead keep all node groups that previously contained a player marked as containing a player 
                ^^^This is a failsafe to prevent random resetting of enemies on borders or in crevices ^^^
         
         */

        foreach (NodeGroup ng in nodeGroups)
        {
            ng.ContainsPlayer = false;
        }

        foreach (Ship s in Ship.PlayerShips)
        {
            foreach (NodeGroup ng in nodeGroups)
            {
                if (ng.nodes.Contains(s.node))
                {
                    ng.ContainsPlayer = true;
                }
            }
        }

        bool atLeastOne = false;
        foreach (NodeGroup ng in nodeGroups)
        {
            if (ng.ContainsPlayer)
                atLeastOne = true;
        }
        if (!atLeastOne)
        {
            foreach (NodeGroup ng in playerNodeGroups)
            {
                ng.ContainsPlayer = true;
            }
        }
        else
        {
            playerNodeGroups.Clear();
            foreach (NodeGroup ng in nodeGroups)
            {
                if (ng.ContainsPlayer)
                {
                    playerNodeGroups.Add(ng);
                }
            }
        }
        

    }
}
