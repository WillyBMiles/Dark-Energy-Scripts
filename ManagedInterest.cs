using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Tooltip("Managed interest will be turned off if out of range of players. DO NOT PLACE ON SHIPS THEY ARE AUTOMATICALLY MANAGED")]
public class ManagedInterest : MonoBehaviour
{
    public Node defaultNode;

    private void Start()
    {
        if (defaultNode == null)
        {
            Debug.LogWarning($"Remember to precalculate! \"{name}\" is missing a node.");
            Precalculate();
        }
    }
    public void Precalculate()
    {
        defaultNode = Node.FindNearestNode(transform.position);
    }
}
