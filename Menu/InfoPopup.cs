using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPopup : MonoBehaviour
{

    public float delay;
    public float fadeTime;
    float currentFade;

    public TextMeshProUGUI text;
    public Image image;
    Color startImageColor;
    // Start is called before the first frame update
    void Start()
    {
        currentFade = fadeTime;
        startImageColor = image.color;
    }

    // Update is called once per frame
    void Update()
    {
        delay -= Time.deltaTime;
        if (delay <= 0)
        {
            currentFade -= Time.deltaTime;

            image.color = new Color(image.color.r, image.color.g, image.color.b, startImageColor.a * currentFade / fadeTime);
            text.color = new Color(text.color.r, text.color.g, text.color.b, currentFade / fadeTime);

            if (currentFade < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
