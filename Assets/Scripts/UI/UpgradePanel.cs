using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private Text upgradeAmountText;
    [SerializeField] private Text upgradePriceText;

    public static event Action<UpgradeType> OnUpgradeButton;

    private void Start()
    {
        Upgrades.OnCurrentUpgradeIndexChange += UpdatePanel;
    }

    public void OnUpgradeButtonPressed()
    {
        OnUpgradeButton?.Invoke(upgradeType);
    }    
    
    private void UpdatePanel(UpgradeType upgradeType, UpgradeData newUpgradeDataData)
    {
        if (upgradeType != this.upgradeType) return;
        upgradeAmountText.text = string.Concat("+", newUpgradeDataData.UpgradeAmount.ToString());
        upgradePriceText.text = newUpgradeDataData.Price.ToString();
    }
}
