using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenBase : MonoBehaviour
{
    [SerializeField] private ScreenType screenType;
    [SerializeField] protected GameObject levelsPanel;
    [SerializeField] protected Slider levelsSlider;

    protected List<LevelUI> levelUIs;


    protected virtual void Awake()
    {

    }
    protected virtual void Start()
    {
        if (screenType != ScreenType.InGame)
        {
            levelUIs = new List<LevelUI>();
            levelUIs.AddRange(levelsPanel.GetComponentsInChildren<LevelUI>());
        }
    }
    public virtual void ToggleVisibility(bool show)
    {
        gameObject.SetActive(show);
    }

    public virtual void UpdateLevelsBar(int currentLevel, float updateInTime = 0f)
    {
        //some rocket science to correctly show levels bar UI
        int levelsBarStart = (currentLevel % 5 == 0) ? (currentLevel - 4) : (currentLevel / 5 * 5 + 1);
        int currentLevelIndex = 0;
        for (int i = 0; i < levelUIs.Count; i++)
        {
            int level = levelsBarStart + i;
            LevelState state = (currentLevel < level) ? LevelState.NotAchieved : ((currentLevel > level) ? LevelState.Achieved : LevelState.Current);
            levelUIs[i].UpdateLevelUI(level);
            if (state == LevelState.Current)
            {
                currentLevelIndex = i;
            }
        }
        if(updateInTime == 0)
        {
            UpdateLevelsBarImmediate(currentLevel, currentLevelIndex, levelsBarStart);
        }
        else
        {
            StartCoroutine(UpdateLevelsSliderAndCurrentLevel(currentLevel, levelsBarStart, currentLevelIndex, updateInTime));
        }
    }

    protected IEnumerator UpdateLevelsSliderAndCurrentLevel(int currentLevel, int levelsBarStart, int currentLevelUIIndex, float updateTime)
    {
        yield return StartCoroutine(UpdateLevelsSliderRoutine(currentLevel, levelsBarStart, updateTime));
        levelUIs[currentLevelUIIndex].UpdateLevelUI(LevelState.Current);
    }

    protected void UpdateLevelsBarImmediate(int currentLevel, int currentLevelIndex, int levelsBarStart)
    {
        float sliderValueDifBetweenLevels = ((float)1 / (levelUIs.Count - 1));
        levelsSlider.value = (currentLevel - levelsBarStart) * sliderValueDifBetweenLevels;
        for(int i = 0; i < levelUIs.Count; i++)
        {
            levelUIs[i].UpdateLevelUI(i < currentLevelIndex ? LevelState.Achieved : (i > currentLevelIndex ? LevelState.NotAchieved : LevelState.Current));
        }
    }

    protected IEnumerator UpdateLevelsSliderRoutine(int currentLevel, int levelsBarStart, float updateTime)
    {
        float sliderValueDifBetweenLevels = ((float)1 / (levelUIs.Count - 1));
        float targetSliderValue = (currentLevel - levelsBarStart) * sliderValueDifBetweenLevels;
        float elapsedTime = 0f;
        int sliderInLevelIndexArea = 0;
        float waitTime = 0.01f;
        float sliderIncrement = (targetSliderValue - levelsSlider.value) / (updateTime / waitTime);
        if(targetSliderValue == 0)
        {
            ReInitLevelsBar(sliderValueDifBetweenLevels, sliderInLevelIndexArea, LevelState.NotAchieved);
        }
        else
        {
            while ((levelsSlider.value < targetSliderValue) && (elapsedTime < updateTime))
            {
                yield return new WaitForSeconds(waitTime);
                elapsedTime += waitTime;
                levelsSlider.value += sliderIncrement;
                ReInitLevelsBar(sliderValueDifBetweenLevels, sliderInLevelIndexArea, LevelState.Achieved);
            }
        }
        levelsSlider.value = targetSliderValue;
    }

    protected void ReInitLevelsBar(float sliderValueDifBetweenLevels, int sliderInLevelIndexArea, LevelState state)
    {
        while (levelsSlider.value >= (sliderValueDifBetweenLevels * sliderInLevelIndexArea))
        {
            levelUIs[sliderInLevelIndexArea].UpdateLevelUI(state);
            sliderInLevelIndexArea++;
        }
    }
}
