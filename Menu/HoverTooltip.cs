using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTooltip : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{

    public Tooltip tooltip;
    public string tooltipText;

    bool isHovering;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isHovering)
        {
            tooltip.SetText(tooltipText);
        }
    }
}
