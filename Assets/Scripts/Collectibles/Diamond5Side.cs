using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond5Side : BaseCollectible
{
    [SerializeField] private Animator anim;
    public override CollectibleType collectibleType => CollectibleType.Diamond5Side;

    public override void GetCollected()
    {
        base.GetCollected();
        Debug.Log(name + " : I am collected!");
        anim.SetTrigger("OnCollected");
        Vibration.Vibrate(5);
    }
}
