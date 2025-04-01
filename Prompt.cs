using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class Prompt : MonoBehaviour
{
    static Prompt instance;
    TextMeshProUGUI text;
    public GameObject parent;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        text = GetComponentInChildren<TextMeshProUGUI>();
        
    }

    int timer = 0;
    // Update is called once per frame
    void Update()
    {
        if (timer <= 0)
        {
            if (instance.parent.activeInHierarchy)
                parent.SetActive(false);
        }
        timer--;
    }

    public static void SetPrompt(string prompt)
    {
        if (!instance)
            return;
        if (!instance.parent.activeInHierarchy)
            instance.parent.SetActive(true);
        instance.timer = 2;
        instance.text.text = prompt;
    }
}
