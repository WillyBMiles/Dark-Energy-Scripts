using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{

    public static FadeIn instance;
    public float time;
    Image image;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        image = GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.b, image.color.b, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Ship.playerShip)
        {
            image.color = new Color(image.color.r, image.color.b, image.color.b, image.color.a - Time.deltaTime / time);
        }
        else
        {
            image.color = new Color(image.color.r, image.color.b, image.color.b, 1f);
        }
    }

    public void ResetFade()
    {
        image.color = new Color(image.color.r, image.color.b, image.color.b, 1f);
    }
}
