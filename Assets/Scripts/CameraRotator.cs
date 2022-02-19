using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{

    [SerializeField] private Vector3 dancePos;
    private Vector3 followRotation;
    private Vector3 followPos;
    private float lerpTime = 1.0f;

    private void Start()
    {
        followRotation = transform.eulerAngles;
        followPos = transform.localPosition;
        FinishLine.OnFinishLine += FinishLine_OnFinishLine;
        LevelEndScreen.Instance.OnTapToContinue += Instance_OnTapToContinue;
    }

    private void OnDestroy()
    {
        FinishLine.OnFinishLine -= FinishLine_OnFinishLine;
        LevelEndScreen.Instance.OnTapToContinue -= Instance_OnTapToContinue;
    }

    private void Instance_OnTapToContinue()
    {
        StopAllCoroutines();
        transform.localPosition = followPos;
        transform.eulerAngles = followRotation;
    }

    private void FinishLine_OnFinishLine()
    {
        Rotate(true);
    }

    void Update()
    {

    }
    private void Rotate(bool toForward)
    {
        StartCoroutine(RotateRoutine());
    }

    private IEnumerator RotateRoutine()
    {
        float elapsedTime = 0f;
        Vector3 center = (followPos + dancePos) * 0.5F;
        Vector3 start = followPos - center;
        Vector3 end = dancePos - center;
        float fracComplete = 0f;
        while (fracComplete < 1)
        {
            yield return new WaitForSeconds(0.01f);
            elapsedTime += 0.01f;
            fracComplete = (elapsedTime) / lerpTime;

            transform.localPosition = Vector3.Slerp(start, end, fracComplete);
            transform.position += center;
            transform.LookAt(PlayerController.Instance.transform);
        }
    }
}
