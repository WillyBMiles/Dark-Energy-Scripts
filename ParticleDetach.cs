using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDetach : MonoBehaviour
{

    [Header("Make sure that Stop Action is Destroy on ParticleSystem")]
    public bool _;
    ParticleSystem system;
    Transform parent;
    Vector3 offset;
    Quaternion rotationOffset;
    Vector3 localScale;

    // Start is called before the first frame update
    void Start()
    {
        system = GetComponent<ParticleSystem>();
        offset = transform.localPosition;
        rotationOffset = transform.localRotation;
        localScale = transform.localScale;
        parent = transform.parent;
        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (parent == null)
        {
            system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }  
        else
        {
            transform.SetPositionAndRotation(parent.TransformPoint(offset),
                parent.rotation * rotationOffset);
            transform.localScale = Vector3.Scale(parent.lossyScale, localScale);
        }
    }

}
