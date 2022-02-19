using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : BaseCollectible
{
    public override CollectibleType collectibleType => CollectibleType.Obstacle;

    public override void GetCollected()
    {
        base.GetCollected();
        Vibration.Vibrate(100);
    }

}
