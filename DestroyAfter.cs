using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    public float time = 0f;
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(MyDestroy), time);
    }


    void MyDestroy()
    {
        Destroy(gameObject);
    }
}
