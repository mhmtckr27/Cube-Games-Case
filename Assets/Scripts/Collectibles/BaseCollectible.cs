using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCollectible : MonoBehaviour, ICollectible
{
    [SerializeField] private AudioClip collectedSound;
    public abstract CollectibleType collectibleType { get; }

    public static event Action<CollectibleType, int> OnCollected;

    public virtual void GetCollected() 
    {
        OnCollected?.Invoke(collectibleType, 1);
        GameManager.Instance.audioSource.clip = collectedSound;
        GameManager.Instance.audioSource.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        GetCollected();
    }
}
