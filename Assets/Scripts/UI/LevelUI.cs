using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Text levelText;
    [SerializeField] private Text levelTextShadow;
    [SerializeField] private Image levelPanelBackground;

    //depending on the params, correct function gets called
    public void UpdateLevelUI(int newLevel, LevelState state)
    {
        UpdateLevelUI(newLevel);
        UpdateLevelUI(state);
    }

    public void UpdateLevelUI(int newLevel)
    {
        levelText.text = newLevel.ToString();
        levelTextShadow.text = newLevel.ToString();
    }

    public void UpdateLevelUI(LevelState state)
    {
        levelText.color = UIManager.Instance.levelUIDataDictionary[state].textColor;
        levelTextShadow.color = UIManager.Instance.levelUIDataDictionary[state].textShadowColor;
        levelPanelBackground.color = UIManager.Instance.levelUIDataDictionary[state].panelBackgroundColor;
        rect.localScale = UIManager.Instance.levelUIDataDictionary[state].panelScale;
    }
}