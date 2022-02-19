using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    [SerializeField] private float loadingTime;
    [SerializeField] private Slider loadingBar;
    private void Start()
    {
        StartCoroutine(WaitForInitializations());
    }

    private IEnumerator WaitForInitializations()
    {
        float elapsedTime = 0f;
        float timeIncrement = 0.01f;
        float sliderIncrement = 1 / (loadingTime / timeIncrement);
        while(elapsedTime < loadingTime)
        {
            yield return new WaitForSeconds(timeIncrement);
            elapsedTime += timeIncrement;
            loadingBar.value += sliderIncrement;
        }
        loadingBar.value = 1;

        SceneManager.LoadSceneAsync(GameManager.Instance.CurrentLevel % 2 == 0 ? 2 : 1);
    }
}
