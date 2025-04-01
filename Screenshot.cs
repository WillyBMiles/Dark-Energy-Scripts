using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Screenshot : SerializedMonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(screenShotKey))
            TakeScreenshot();
    }

    public string fileName = "screenshot.png";
    public KeyCode screenShotKey;
    [Button]
    void TakeScreenshot()
    {
        ScreenCapture.CaptureScreenshot(fileName);
    }

}
