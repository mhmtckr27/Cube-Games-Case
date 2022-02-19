using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TapToPlayScreen : ScreenBase, IPointerDownHandler
{
    public static event Action OnTapToPlay;

    protected override void Start()
    {
        base.Start();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        OnTapToPlay?.Invoke();
    }

    public override void ToggleVisibility(bool enable)
    {
        gameObject.SetActive(enable);
        if (enable)
        {
            //UpdateLevelsBar(GameManager.Instance.CurrentLevel);
        }
    }
}

public enum LevelState
{
    NotAchieved,
    Achieved,
    Current
}

public enum UpgradeType
{
    Health,
    Earning
}