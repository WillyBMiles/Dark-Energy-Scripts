using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TinyRocking : MonoBehaviour
{
    public float movementAmount;
    public float driftSpeed;

    Vector3 startingPosition;
    // Start is called before the first frame update
    void Start()
    {
        startingPosition = transform.position;
    }

    
    // Update is called once per frame
    void Update()
    {
        transform.position = startingPosition + movementAmount * (Mathf.Cos(Time.time * driftSpeed) * Vector3.right + Mathf.Sin(Time.time * driftSpeed) * Vector3.up);
    }
}
