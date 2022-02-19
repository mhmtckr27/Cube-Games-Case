using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameScreen : ScreenBase
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private LevelUI levelUI;
    [SerializeField] public GameObject currentLevelPanel;
    [SerializeField] private Slider diamondProgress;
    [SerializeField] private TextMeshProUGUI diamondText;

    bool shouldUpdateProgressBar;

    public static InGameScreen Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected override void Start()
    {
        base.Start();
        TapToPlayScreen.OnTapToPlay += OnLevelStart;
        FinishLine.OnFinishLine += FinishLine_OnFinishLine;
        GameManager.Instance.OnCollectedDiamondChange += Instance_OnCollectedDiamondChange;
    }

    private void Instance_OnCollectedDiamondChange(int collectedDiamond)
    {
        diamondText.text = collectedDiamond.ToString();
        diamondProgress.value = ((float)collectedDiamond / GameManager.Instance.maxStackLimit);
    }

    private void FinishLine_OnFinishLine()
    {
        shouldUpdateProgressBar = false;
        StopAllCoroutines();
        progressBar.value = 1;
        diamondProgress.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        TapToPlayScreen.OnTapToPlay -= OnLevelStart;
        FinishLine.OnFinishLine -= FinishLine_OnFinishLine;
        GameManager.Instance.OnCollectedDiamondChange -= Instance_OnCollectedDiamondChange;
    }

    private void OnLevelStart()
    {
        levelUI.UpdateLevelUI(GameManager.Instance.CurrentLevel);
        shouldUpdateProgressBar = true;
        diamondProgress.gameObject.SetActive(true);
        Invoke(nameof(UpgradeProgressBarWrapper), 0.025f);
    }

    private void UpgradeProgressBarWrapper()
    {
        StartCoroutine(UpdateProgressBarRoutine());
    }

    private IEnumerator UpdateProgressBarRoutine()
    {
        while (shouldUpdateProgressBar)
        {
            progressBar.value = PlayerController.Instance.transform.position.z / (GameManager.Instance.levelEndPointZ - GameManager.Instance.levelStartPointZ);
            yield return new WaitForSeconds(0.02f);
        }
    }
}
