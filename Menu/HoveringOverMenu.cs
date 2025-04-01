using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoveringOverMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsHovering { get { return current != null; } }
    static HoveringOverMenu current;
    public bool whileEnabled = false;

    private void Update()
    {
        if (whileEnabled)
        {
            current = this;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        current = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (current == this)
            current = null;
    }

    private void OnDisable()
    {
        if (current == this)
            current = null;
    }
}
