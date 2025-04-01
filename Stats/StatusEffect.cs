using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class StatusEffect 
{
    public string name;

    public bool simpleDuration;

    [Tooltip("Should probably be 10f")]
    [ShowIf("@buildUp || simpleDuration")]
    public float duration = 10f;

    public List<StatAdjustment> statAdjustments = new();

    public GameObject particleEffect;

}

public enum StatusType
{
    Core,
    Stabilization,
    Systems,

    Synergy, //Specifically for boosts
}