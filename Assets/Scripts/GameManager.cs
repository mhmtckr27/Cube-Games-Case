using Defective.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using com.HayriCakir;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string jsonFileName;
    [SerializeField] private List<GameObject> dontDestroyOnLoadObjects;
    [SerializeField] private Upgrades upgrades;
    [SerializeField] private List<int> platformCounts;

    [Header("PREFABS")] [Space]
    [SerializeField] private List<string> prefabNames;
    [SerializeField] private List<GameObject> prefabs;

    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip upgradeSound;
    [SerializeField] public int maxStackLimit;

    private List<Road> currentLevelPlatforms;
    public float levelStartPointZ;
    public float levelEndPointZ;

    public int highScore;
    private int finalScoreThisLevel;
    public int currentHealth;
    public Dictionary<UpgradeType, int> currentUpgradedValues;
    private bool isLevelStarted;
    public bool isGameOver;

    private int collectedDiamondThisLevel;
    public int CurrentLevel { get; private set; }

    private int goldAmount;
    public int GoldAmount
    {
        get => goldAmount;
        set
        {
            goldAmount = value;
            OnGoldChange?.Invoke(goldAmount);
        }
    }

    private Dictionary<string, GameObject> spawnablePrefabs;

    public event Action<int> OnGoldChange;
    public event Action<bool, int, int, int> OnIsNewHighScore;
    public event Action<int> OnCollectedDiamondChange;
    public event Action<int> OnCurrentHealthChange;
    public event Action<UpgradeType, int> OnSuccessfulUpgrade;
    public event Action<int, int> OnGameOver;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        spawnablePrefabs = new Dictionary<string, GameObject>();
        for (int i = 0; i < prefabNames.Count; i++)
        {
            spawnablePrefabs.Add(prefabNames[i], prefabs[i]);
        }

        upgrades.Init();

        currentUpgradedValues = new Dictionary<UpgradeType, int>
        {
            { UpgradeType.Health, 3},
            { UpgradeType.Earning, 1 }
        };
        OnSuccessfulUpgrade?.Invoke(UpgradeType.Health, 3);
    }
    
    private void Start()
    {
        foreach (GameObject gameObject in dontDestroyOnLoadObjects)
        {
            DontDestroyOnLoad(gameObject);
        }

        TapToPlayScreen.OnTapToPlay += StartLevel;
        FinishLine.OnFinishLine += StopLevel;
        LevelEndScreen.Instance.OnTapToContinue += LevelEndScreen_OnTapToContinue;
        BaseCollectible.OnCollected += OnCollectedItem;
        UpgradePanel.OnUpgradeButton += UpgradePanel_OnUpgradeButton;
        
        LoadGameData();

    }
    
    private void OnDestroy()
    {
        TapToPlayScreen.OnTapToPlay -= StartLevel;
        FinishLine.OnFinishLine -= StopLevel;
        LevelEndScreen.Instance.OnTapToContinue -= LevelEndScreen_OnTapToContinue;
        Diamond.OnCollected -= OnCollectedItem;
        UpgradePanel.OnUpgradeButton -= UpgradePanel_OnUpgradeButton;
    }

    private void OnCollectedItem(CollectibleType collectedType, int amountToAdd)
    {
        switch (collectedType)
        {
            case CollectibleType.Obstacle:
                currentHealth--;
                OnCurrentHealthChange?.Invoke(currentHealth);
                if (currentHealth == 0)
                {
                    isLevelStarted = false;
                    isGameOver = true;
                    PlayerController.Instance.StartRunning(false, true);
                    LevelEndScreen.Instance.animator.SetBool("IsLevelSuccess", false);
                    OnGameOver?.Invoke(collectedDiamondThisLevel * currentUpgradedValues[UpgradeType.Earning], currentUpgradedValues[UpgradeType.Earning]);
                }
                break;
            case CollectibleType.Diamond5Side:
            case CollectibleType.Diamond:
                collectedDiamondThisLevel += amountToAdd;
                PlayerController.Instance.plusOneVFX.Play();
                break;
        }
        OnCollectedDiamondChange?.Invoke(collectedDiamondThisLevel);
    }

    private void OnLevelWasLoaded(int level)
    {
        collectedDiamondThisLevel = 0;
        OnCollectedDiamondChange?.Invoke(0);
        PlayerController.Instance.transform.position = Vector3.zero;
        SpawnPlatforms();
        SpawnCollectablesFromJSON();
    }

    private void UpgradePanel_OnUpgradeButton(UpgradeType upgradeType)
    {
        UpgradeData upgradeData = upgrades.GetUpgradeData(upgradeType);
        if (goldAmount < upgradeData.Price) { return; }
        currentUpgradedValues[upgradeType] += upgradeData.UpgradeAmount;
        GoldAmount -= upgradeData.Price;
        upgrades.UpdateUpgradeIndex(upgradeType);
        audioSource.clip = upgradeSound;
        audioSource.Play();
        OnSuccessfulUpgrade?.Invoke(upgradeType, currentUpgradedValues[upgradeType]);
    }

    private void LevelEndScreen_OnTapToContinue()
    {
        if (isGameOver)
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            CurrentLevel++;
            int sceneToLoad = (SceneManager.GetActiveScene().buildIndex + 1) == SceneManager.sceneCountInBuildSettings ? 1 : (SceneManager.GetActiveScene().buildIndex + 1);
            SceneManager.LoadSceneAsync(sceneToLoad);
        }
    }

    private void StopLevel()
    {
        if (isLevelStarted == true)
        {
            PlayerController.Instance.StartRunning(false, false);
            isLevelStarted = false;
            LevelEndScreen.Instance.animator.SetBool("IsLevelSuccess", true);
            //TODO: add a score bonus multiplier or sth idk.
            finalScoreThisLevel = collectedDiamondThisLevel * currentUpgradedValues[UpgradeType.Earning];
            goldAmount += finalScoreThisLevel;
            OnIsNewHighScore?.Invoke(highScore < finalScoreThisLevel, finalScoreThisLevel, currentUpgradedValues[UpgradeType.Earning], highScore);
            if (highScore < finalScoreThisLevel)
            {
                highScore = finalScoreThisLevel;
            }
            SaveGameData();
        }
    }

    private void StartLevel()
    {
        if (isLevelStarted == false)
        {
            currentHealth = currentUpgradedValues[UpgradeType.Health];
            isGameOver = false;
            OnCurrentHealthChange?.Invoke(currentHealth);
            PlayerController.Instance.StartRunning(true, false);
            isLevelStarted = true;
        }
    }

    private void SpawnPlatforms()
    {
        currentLevelPlatforms = new List<Road>();

        int platformCount = platformCounts[SceneManager.GetActiveScene().buildIndex]; 

        currentLevelPlatforms.Add(Instantiate(spawnablePrefabs["Road"], new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Road>());
        BoxCollider platformCollider = currentLevelPlatforms[0].GetComponentInChildren<Road>().boxCollider;

        for(int i = 1; i < platformCount; i++)
        {
            currentLevelPlatforms.Add(Instantiate(spawnablePrefabs["Road"], new Vector3(0, 0, i * platformCollider.bounds.size.z), Quaternion.identity).GetComponent<Road>());
        }

        Instantiate(spawnablePrefabs["NonCollidingPlatform"], new Vector3(0, 0, -platformCollider.bounds.size.z), Quaternion.identity);
        levelStartPointZ = currentLevelPlatforms[0].boxCollider.bounds.min.z;
        levelEndPointZ = currentLevelPlatforms.Count * platformCollider.bounds.size.z;
        Instantiate(spawnablePrefabs["Road"], new Vector3(0, 0, levelEndPointZ), Quaternion.identity);
        Vector3 finishLinePos = currentLevelPlatforms[currentLevelPlatforms.Count - 1].transform.position;
        finishLinePos.z = levelEndPointZ;
        Instantiate(spawnablePrefabs["FinishLine"], finishLinePos, Quaternion.identity);
    }

    public void SpawnCollectablesFromJSON()
    {
        string jsonStr = Resources.Load<TextAsset>(jsonFileName).text;
        JSONObject jSONObject = new JSONObject(jsonStr);

        float columnStartPos = currentLevelPlatforms[1].boxCollider.bounds.min.x + .3f;
        float rowStartPos = currentLevelPlatforms[1].boxCollider.bounds.min.z + .25f;
        float columnEndPos = currentLevelPlatforms[currentLevelPlatforms.Count - 2].boxCollider.bounds.max.x - .3f;
        float rowEndPos = currentLevelPlatforms[currentLevelPlatforms.Count - 2].boxCollider.bounds.max.z - .25f;
        int rowCount = jSONObject.list[SceneManager.GetActiveScene().buildIndex].count;
        int columnCount = jSONObject.list[SceneManager.GetActiveScene().buildIndex][0].count;

        int[,] jsonMatrix = new int[rowCount, columnCount];

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                jsonMatrix[i, j] = jSONObject.list[SceneManager.GetActiveScene().buildIndex][i][j].intValue;
            }
        }

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                if(jsonMatrix[i, j] == 0)
                {
                    continue;
                }
                Vector3 spawnPos = new Vector3();
                GameObject prefabToSpawn = spawnablePrefabs[((CollectibleType)jsonMatrix[i, j]).ToString()];
                spawnPos.x = Mathf.Lerp(columnStartPos, columnEndPos, (float)j / (columnCount - 1));
                spawnPos.y = prefabToSpawn.transform.position.y;
                spawnPos.z = Mathf.Lerp(rowStartPos, rowEndPos, (float)i / (rowCount - 1));
                Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    private void SaveGameData()
    {
        PlayerPrefs.SetInt("CurrentLevel", CurrentLevel);
        PlayerPrefs.SetInt("GoldAmount", GoldAmount);
        PlayerPrefs.SetInt("HighScore", highScore);
        foreach (UpgradeType upgradeType in currentUpgradedValues.Keys)
        {
            PlayerPrefs.SetInt("CurrentUpgraded" + upgradeType.ToString() + "Value", currentUpgradedValues[upgradeType]);
        }
        upgrades.Save();
        PlayerPrefs.Save();
    }

    private void LoadGameData()
    {
        CurrentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        GoldAmount = PlayerPrefs.GetInt("GoldAmount", 0);
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        foreach (UpgradeType upgradeType in System.Enum.GetValues(typeof(UpgradeType)))
        {
            currentUpgradedValues[upgradeType] = PlayerPrefs.HasKey("CurrentUpgraded" + upgradeType.ToString() + "Value") 
                                                 ? PlayerPrefs.GetInt("CurrentUpgraded" + upgradeType.ToString() + "Value") 
                                                 : currentUpgradedValues[upgradeType];
        }
        upgrades.Load();
    }
}

[System.Serializable]
public struct UpgradeData
{
    public int UpgradeAmount;
    public int Price;
}

[System.Serializable]
public struct Upgrades
{
    public List<UpgradeData> healthUpgrades;
    public List<UpgradeData> earningUpgrades;

    private Dictionary<UpgradeType, List<UpgradeData>> upgrades;
    private Dictionary<UpgradeType, int> currentUpgradeIndices;
    
    public static event Action<UpgradeType, UpgradeData> OnCurrentUpgradeIndexChange;

    public void Init()
    {
        upgrades = new Dictionary<UpgradeType, List<UpgradeData>>
        {
            { UpgradeType.Health, healthUpgrades },
            { UpgradeType.Earning, earningUpgrades }
        };        
        
        currentUpgradeIndices = new Dictionary<UpgradeType, int>
        {
            { UpgradeType.Health, 0 },
            { UpgradeType.Earning, 0 }
        };
    }

    public UpgradeData GetUpgradeData(UpgradeType upgradeType)
    {
        return upgrades[upgradeType][Mathf.Clamp(currentUpgradeIndices[upgradeType], 0, upgrades[upgradeType].Count - 1)];
    }

    public void UpdateUpgradeIndex(UpgradeType upgradeType)
    {
        currentUpgradeIndices[upgradeType]++;
        OnCurrentUpgradeIndexChange?.Invoke(upgradeType, GetUpgradeData(upgradeType));
    }

    public void Save()
    {
        foreach (UpgradeType upgradeType in upgrades.Keys)
        {
            PlayerPrefs.SetInt(upgradeType.ToString(), currentUpgradeIndices[upgradeType]);
        }
    }

    public void Load()
    {
        foreach (UpgradeType upgradeType in upgrades.Keys)
        {
            currentUpgradeIndices[upgradeType] = PlayerPrefs.GetInt(upgradeType.ToString(), 0);
            OnCurrentUpgradeIndexChange?.Invoke(upgradeType, GetUpgradeData(upgradeType));
        }
    }
}

public struct SpawnablePrefabs
{
    public string prefabName;
    public GameObject prefab;
}

public class SpawnMatrixClass
{
    public int[][] _matrix;
}