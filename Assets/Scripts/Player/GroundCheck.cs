using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public static event Action<Collider> OnCurrentPlatformChange;
    private void OnTriggerEnter(Collider other)
    {
        OnCurrentPlatformChange?.Invoke(other);
    }
}
