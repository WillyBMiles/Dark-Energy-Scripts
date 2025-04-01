using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConfirmPopup : MonoBehaviour
{
    public static ConfirmPopup instance;

    public TextMeshProUGUI textBox;
    public GameObject parent;
    public delegate void Confirm();


    public Confirm ConfirmYes;
    public Confirm ConfirmNo;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    float timer = 0;
    // Update is called once per frame
    void Update()
    {
        if (timer <= 0)
        {
            Clear();
        }
        else
            timer -= Time.deltaTime;
    }

    public void Show(string text, Confirm yesCallback, Confirm noCallback, float time)
    {
        ConfirmYes = yesCallback;
        ConfirmNo = noCallback;

        textBox.text = text;
        parent.SetActive(true);
        timer = time;
    }


    public void Yes()
    {
        ConfirmYes?.Invoke();
        Clear();

    }

    public void No()
    {
        ConfirmNo?.Invoke();
        Clear();
    }

    public void Clear()
    {
        ConfirmYes = null;
        ConfirmNo = null;
        parent.SetActive(false);
    }


}
