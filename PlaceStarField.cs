using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlaceStarField : SerializedMonoBehaviour
{
    public int minX = -20;
    public int maxX = 20;
    public int minY = -20;
    public int maxY = 20;
    public float maxJiggle = 0f;
    public float density = 1;
    public List<GameObject> possibleStars = new();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button]
    void Place()
    {
        for (float x = minX; x < maxX; x+= 1f/ density)
        {
            for (float y = minY; y < maxY; y+= 1f/ density)
            {
                GameObject go = Instantiate(possibleStars[Random.Range(0, possibleStars.Count)], transform);
                go.transform.position = new Vector3(x, y) + new Vector3(Random.Range(-maxJiggle, maxJiggle), Random.Range(-maxJiggle, maxJiggle));
            }
        }
    }
    [Button]
    void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
