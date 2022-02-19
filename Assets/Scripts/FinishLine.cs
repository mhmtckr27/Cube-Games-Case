using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public static event Action OnFinishLine;

    private void OnTriggerEnter(Collider other)
    {
        Invoke(nameof(OnFinishLineWrapper), 0.25f);
    }

    private void OnFinishLineWrapper()
    {
        OnFinishLine?.Invoke();
    }
}
