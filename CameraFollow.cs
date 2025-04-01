using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    static CameraFollow instance;

    public Transform target;
    public float smoothTime;
    float z;
    // Start is called before the first frame update
    void Start()
    {
        z = transform.position.z;
        instance = this;
        //transform.position = target.transform.position;
        //transform.position = new Vector3(transform.position.x, transform.position.y, z);
    }

    Vector3 velocity;
    // Update is called once per frame
    void Update()
    {
        if (Ship.playerShip != null)
        {
            target = Ship.playerShip.transform;
            if (Ship.playerShip.playerDisabled)
            {
                foreach (Ship s in Ship.PlayerShips)
                {
                    if (!s.playerDisabled)
                    {
                        target = s.transform;
                        break;
                    }
                }
            }
        }
            
        if (target != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, smoothTime);
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }
        
    }

    public static void SetCamera(Vector3 point)
    {
        if (instance != null)
        {
            instance.transform.position = new Vector3(point.x, point.y, instance.z); ;
        }
    }
}
