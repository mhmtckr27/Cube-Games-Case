using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private LevelUIData notAchievedLevelUIData;
    [SerializeField] private LevelUIData achievedLevelUIData;
    [SerializeField] private LevelUIData currentLevelUIData;
    [SerializeField] private TextMeshProUGUI goldAmountText;
    [SerializeField] private Text fpsText;
    [SerializeField] private TapToPlayScreen tapToPlayScreen;
    [SerializeField] private InGameScreen inGameScreen;
    [SerializeField] private LevelEndScreen levelEndScreen;
    [SerializeField] private GameObject levelsPanel;
    [SerializeField] private AudioSource audioSource;

    Dictionary<ScreenType, ScreenBase> screens;
    public Dictionary<LevelState, LevelUIData> levelUIDataDictionary;
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void PopulateLevelUIDataDictionary()
    {
        levelUIDataDictionary = new Dictionary<LevelState, LevelUIData>();
        levelUIDataDictionary.Add(LevelState.NotAchieved, notAchievedLevelUIData);
        levelUIDataDictionary.Add(LevelState.Achieved, achievedLevelUIData);
        levelUIDataDictionary.Add(LevelState.Current, currentLevelUIData);
    }
    private void OnEnable()
    {
        GameManager.Instance.OnGoldChange += UpdateGoldText;
        TapToPlayScreen.OnTapToPlay += OnLevelStarted;
        FinishLine.OnFinishLine += OnFinishLine;
        levelEndScreen.OnTapToContinue += LevelEndScreen_OnTapToContinue;
        UpdateGoldTextStateMachineBehaviour.OnUpdateGoldText += UpdateGoldTextStateMachineBehaviour_OnUpdateGoldText;
    }

    private void Start()
    {
        PopulateScreensDictionary();
        PopulateLevelUIDataDictionary();

        tapToPlayScreen.UpdateLevelsBar(GameManager.Instance.CurrentLevel);
        ToggleScreens(ScreenType.TapToPlay);
    }

    private void UpdateGoldTextStateMachineBehaviour_OnUpdateGoldText()
    {
        StartCoroutine(UpdateGoldTextRoutine(GameManager.Instance.GoldAmount));
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGoldChange -= UpdateGoldText;
        TapToPlayScreen.OnTapToPlay -= OnLevelStarted;
        FinishLine.OnFinishLine -= OnFinishLine;
        levelEndScreen.OnTapToContinue -= LevelEndScreen_OnTapToContinue;
        UpdateGoldTextStateMachineBehaviour.OnUpdateGoldText -= UpdateGoldTextStateMachineBehaviour_OnUpdateGoldText;
    }

    private void LevelEndScreen_OnTapToContinue()
    {
        goldAmountText.text = GameManager.Instance.GoldAmount.ToString();
    }

    private void OnFinishLine()
    {
        ToggleScreens(ScreenType.LevelEnd);
    }

    private void PopulateScreensDictionary()
    {
        screens = new Dictionary<ScreenType, ScreenBase>();
        screens.Add(ScreenType.TapToPlay, tapToPlayScreen);
        screens.Add(ScreenType.InGame, inGameScreen);
        screens.Add(ScreenType.LevelEnd, levelEndScreen);
    }

    private void OnLevelStarted()
    {
        ToggleScreens(ScreenType.InGame);
    }

    private void OnLevelWasLoaded(int level)
    {
        ToggleScreens(ScreenType.TapToPlay);
    }

    private void UpdateGoldText(int newGoldAmount)
    {
        goldAmountText.text = newGoldAmount.ToString();
    }

    public IEnumerator UpdateGoldTextRoutine(int newGoldAmount)
    {
        List<int> scoreDigits = new List<int>();
        int tempScore = newGoldAmount;
        while (tempScore > 0)
        {
            scoreDigits.Add(tempScore % 10);
            tempScore /= 10;
        }

        if(scoreDigits.Count > goldAmountText.text.Length)
        {
            string empty = "";
            for(int i = 0; i < (scoreDigits.Count - goldAmountText.text.Length); i++)
            {
                empty = string.Concat(empty, "0");
            }
            goldAmountText.text = goldAmountText.text.Insert(0, empty);
        }

        audioSource.Play();
        for (int i = 0; i < scoreDigits.Count; i++)
        {
            StartCoroutine(UpdateGoldTextDigit(scoreDigits.Count - 1 - i, scoreDigits[i]));
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator UpdateGoldTextDigit(int digitIndex, int newValue)
    {
        int currentValue;
        char currentChar = goldAmountText.text.ToCharArray()[digitIndex];
        if (currentChar == ' ')
        {
            currentValue = 0;
        }
        else
        {
            currentValue = (int)char.GetNumericValue(currentChar);
        }
        char[] newStr;
        while(currentValue != newValue)
        {
            yield return new WaitForSeconds(0.05f);
            currentValue = (currentValue + 1) > 9 ? 0 : currentValue + 1;
            newStr = goldAmountText.text.ToCharArray();
            newStr[digitIndex] = currentValue.ToString().ToCharArray()[0];
            goldAmountText.text = newStr.ArrayToString();
        }
        newStr = goldAmountText.text.ToCharArray();
        newStr[digitIndex] = currentValue.ToString().ToCharArray()[0];
        goldAmountText.text = newStr.ArrayToString();
    }

    private void ToggleScreens(ScreenType screenTypeToShow)
    {
        foreach (ScreenType screenType in screens.Keys)
        {
            levelsPanel.SetActive((screenTypeToShow == ScreenType.TapToPlay));
            screens[screenType].ToggleVisibility((screenType == screenTypeToShow) || (screenTypeToShow == ScreenType.LevelEnd && screenType == ScreenType.InGame));
        }
    }
}

public enum ScreenType
{
    TapToPlay,
    InGame,
    LevelEnd
}

[System.Serializable]
public struct LevelUIData
{
    public Color textColor;
    public Color textShadowColor;
    public Color panelBackgroundColor;
    public Vector2 panelScale;
}