using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPopupManager : MonoBehaviour
{
    static InfoPopupManager instance;
    public Transform parent;
    public InfoPopup popup;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void MakePopup(string text)
    {
        InfoPopup popup = Instantiate(instance.popup, instance.parent);
        popup.text.text = text;
    }
}
