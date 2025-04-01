using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RectTransform healthBar;
    public RectTransform healthBarBack;
    public float widthMultHealth;
    
    public RectTransform powerBar;
    public RectTransform powerBarBack;
    public float widthMultPowerBar;

    Ship playerShip;

    [Space(20)]
    public GameObject cellPrefab;
    public Transform parent;

    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI differenceText;

    public Image toolIcon;
    public Image leftIcon;
    public Image rightIcon;

    List<GameObject> cells = new();

    [Space(20)]
    public Image prefabForToolIcons;
    public List<Image> toolIcons = new();
    public Transform toolIconParent;
    public List<TextMeshProUGUI> ButtonIndicators = new();
    public GameObject allToolIcons;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerShip = Ship.playerShip;
        if (playerShip == null)
        {
            healthBar.sizeDelta = new Vector2(0f, healthBarBack.sizeDelta.y);
            return;
        }
        UpdateCurrencyText();
            
        healthBarBack.sizeDelta = new Vector2(widthMultHealth * playerShip.ActualMaxHealth, healthBarBack.sizeDelta.y);
        healthBar.sizeDelta = new Vector2(widthMultHealth * playerShip.health, healthBarBack.sizeDelta.y);

        powerBarBack.sizeDelta = new Vector2(widthMultPowerBar * playerShip.ActualMaxPower, healthBarBack.sizeDelta.y);
        powerBar.sizeDelta = new Vector2(widthMultPowerBar * playerShip.power, healthBarBack.sizeDelta.y);

        if (cells.Count > playerShip.cells)
        {
            while (cells.Count > 0)
            {
                Destroy(cells[0]);
                cells.RemoveAt(0);
            }
        }

        for (int i=cells.Count; i < playerShip.cells; i++)
        {
            cells.Add(Instantiate(cellPrefab, parent));
        }

        if (PlayerItems.localItems)
        {
            PlayerItems.localItems.GetCurrentTool(out ItemInfo itemInfo);
            Item.AssignIcon(itemInfo, toolIcon, false);
            Item.AssignIcon(PlayerItems.localItems.weapon1, leftIcon, false);
            Item.AssignIcon(PlayerItems.localItems.weapon2, rightIcon, false);
            if (PlayerItems.localItems.tools.Count != toolIcons.Count)
            {
                UpdateToolIcons();
            }

            bool foundATool = false;

            for (int i =0; i < PlayerItems.localItems.tools.Count; i++)
            {
                if (PlayerItems.localItems.tools[i] != null)
                    foundATool = true;

                if (i == PlayerItems.localItems.currentTool)
                {
                    ButtonIndicators[i].text = $"{i + 1}<";
                }
                else
                    ButtonIndicators[i].text = $"{i + 1}";
                Item.AssignIcon(PlayerItems.localItems.tools[i], toolIcons[i], false);
            }

            if (!foundATool)
                allToolIcons.SetActive(false);
            else
                allToolIcons.SetActive(true);
        }
        
    }

    public void UpdateToolIcons()
    {
        while (toolIcons.Count > 0)
        {
            Destroy(toolIcons[0].transform.parent.gameObject);
            toolIcons.RemoveAt(0);
        }
        foreach (TextMeshProUGUI go in ButtonIndicators)
        {
            go.gameObject.SetActive(false);
        }

        for (int i=0; i< PlayerItems.localItems.tools.Count; i++)
        {
            toolIcons.Add(Instantiate(prefabForToolIcons, toolIconParent).transform.GetChild(0).GetComponent<Image>());
            if (ButtonIndicators.Count > i)
                ButtonIndicators[i].gameObject.SetActive(true);
        }
    }

    int targetCurrency;
    int currentCurrency;
    int currentDifference;
    int lastStartingCurrency;
    float lastTime;
    const float COUNT_SPEED_MULT = 1f;
    void UpdateCurrencyText()
    {
        int actual = playerShip.GetComponent<PlayerRPG>().currency;
        if (targetCurrency != actual)
        {
            if (targetCurrency < actual)
            {
                currentDifference += actual - targetCurrency;
                lastStartingCurrency = currentCurrency;
                lastTime = 0f;
            }
            else
            {
                currentDifference = 0;
                currentCurrency = targetCurrency;
            }
            targetCurrency = actual;
        }
        if (currentCurrency == targetCurrency)
        {
            currentDifference = 0;
        }
        else
        {
            currentCurrency = (int) Mathf.Lerp(lastStartingCurrency, targetCurrency, lastTime);
            lastTime += Time.deltaTime * COUNT_SPEED_MULT;
        }
        

        currencyText.text = $"{currentCurrency:n0}";

        if (currentDifference == 0)
        {
            differenceText.text = "";
        }
        else
        {
            differenceText.text = "";
            
            if (currentDifference > 0)
            {
                differenceText.text = "+"; 
            }

            differenceText.text += $"{currentDifference:n0}";
        }
    }
}
