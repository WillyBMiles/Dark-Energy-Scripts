using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class InheritRectTransform : MonoBehaviour
{
    [SerializeField]
    RectTransform from;

    RectTransform myRect;

    public bool verticalScale;
    public bool horizontalScale;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (myRect == null)
            myRect = GetComponent<RectTransform>();

        if (verticalScale)
        {
            myRect.sizeDelta = new Vector2(myRect.sizeDelta.x, from.sizeDelta.y);
        }
        if (horizontalScale)
        {
            myRect.sizeDelta = new Vector2(from.sizeDelta.x, myRect.sizeDelta.y);
        }
    }
}
