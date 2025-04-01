using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IndividualLevelUI : MonoBehaviour
{
    static IndividualLevelUI currentHover;

    public Stat.StatEnum stat;

    public TextMeshProUGUI text;
    public TextMeshProUGUI levelText;
    LevelUI levelUI;

    public UnityEngine.UI.Button plusButton;
    public UnityEngine.UI.Button minusButton;

    public Tooltip tooltip;


    // Start is called before the first frame update
    void Start()
    {
        levelUI = GetComponentInParent<LevelUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = Stat.allStats[stat].name;

        plusButton.interactable = levelUI.CanAdd(stat);
        minusButton.interactable = levelUI.CanSubtract(stat);

        levelText.text = levelUI.GetLevel(stat).ToString();

        if (currentHover == this)
        {
            tooltip.SetText($"<b>{Stat.allStats[stat].name} ({Stat.allStats[stat].id})</b>\n{Stat.allStats[stat].description}");
        }
    }

    public void Plus()
    {
        levelUI.AddLevel(stat);
    }

    public void Minus()
    {
        levelUI.RemoveLevel(stat);
    }

    public void StartHover()
    {
        currentHover = this;
    }
    public void EndHover()
    {
        if (currentHover == this)
            currentHover = null;
    }

}
