using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    IndividualLevelUI ilu;
    // Start is called before the first frame update
    void Start()
    {
        ilu = GetComponentInParent<IndividualLevelUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ilu.StartHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ilu.EndHover();
    }
}
