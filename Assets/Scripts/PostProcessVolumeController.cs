using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessVolumeController : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;

    VolumeParameter<Color> param = new VolumeParameter<Color>
    {
        value = Color.black
    };
    VolumeParameter<float> param2 = new VolumeParameter<float>
    {
        value = 0.25f
    };

    private void Start()
    {
        BaseCollectible.OnCollected += BaseCollectible_OnCollected;
        (globalVolume.sharedProfile.components[2] as VolumeComponent).parameters[0].SetValue(param);
        (globalVolume.sharedProfile.components[2] as VolumeComponent).parameters[1].overrideState = false;
        (globalVolume.sharedProfile.components[2] as VolumeComponent).parameters[2].SetValue(param2);
    }

    private void OnDestroy()
    {
        BaseCollectible.OnCollected -= BaseCollectible_OnCollected;
    }
    
    private void BaseCollectible_OnCollected(CollectibleType collectible, int amount)
    {
        if (collectible == CollectibleType.Obstacle)
        {
            StartCoroutine(GetDamageRoutine());
        }
    }

    private IEnumerator GetDamageRoutine()
    {
        param.value = Color.red;
        param2.value = 1f;
        (globalVolume.sharedProfile.components[2] as VolumeComponent).parameters[0].SetValue(param);
        (globalVolume.sharedProfile.components[2] as VolumeComponent).parameters[1].overrideState = true;
        (globalVolume.sharedProfile.components[2] as VolumeComponent).parameters[2].SetValue(param2);
        yield return new WaitForSeconds(0.15f);
        param.value = Color.black;
        param2.value = 0.25f;
        (globalVolume.sharedProfile.components[2] as VolumeComponent).parameters[0].SetValue(param);
        (globalVolume.sharedProfile.components[2] as VolumeComponent).parameters[1].overrideState = false;
        (globalVolume.sharedProfile.components[2] as VolumeComponent).parameters[2].SetValue(param2);
    }
}
