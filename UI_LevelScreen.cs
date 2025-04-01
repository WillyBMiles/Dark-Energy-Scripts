using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_LevelScreen : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    LevelUI uiController;

    // Start is called before the first frame update
    void Start()
    {
        uiController = GetComponent<LevelUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerRPG.localRpg == null)
            return;
        string add = uiController.levelOffset > 0 ? "*" : "";
        levelText.text = (PlayerRPG.localRpg.level + uiController.levelOffset) + add + "\n"
            + PlayerRPG.GetCost(PlayerRPG.localRpg.level + uiController.levelOffset) + "\n"
            + (PlayerRPG.localRpg.currency - uiController.cost) + add;
    }
}
