using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPanel : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup healthsGrid;
    [SerializeField] private GameObject healthImage;
    [SerializeField] private Color defaultHealthColor;
    [SerializeField] private Color lostHealthColor;

    private List<Image> healthImages;
    

    private void Start()
    {
        healthImages = new List<Image>();
        GameManager.Instance.OnCurrentHealthChange += Instance_OnCurrentHealthChange;
        TapToPlayScreen.OnTapToPlay += TapToPlayScreen_OnTapToPlay;
    }

    private void TapToPlayScreen_OnTapToPlay()
    {
        int totalHealth = GameManager.Instance.currentUpgradedValues[UpgradeType.Health] - transform.childCount;
        for (int i = 0; i < totalHealth; i++)
        {
            healthImages.Add(Instantiate(healthImage, healthsGrid.transform).GetComponent<Image>());
        }
    }

    private void Instance_OnCurrentHealthChange(int newHealth)
    {
        for (int i = 0; i < healthImages.Count; i++)
        {
            healthImages[i].color = newHealth > i ? defaultHealthColor : lostHealthColor;
        }
    }
}
