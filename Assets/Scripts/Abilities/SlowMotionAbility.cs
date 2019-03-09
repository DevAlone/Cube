using System.Collections;
using UnityEngine;

public class SlowMotionAbility : BaseAbility
{
    public float speedModifier;

    protected override IEnumerator Activate()
    {
        gameController.gameSpeedModifier = speedModifier;
        yield return new WaitForSeconds(durationSeconds);
        gameController.ResetGameSpeedModifier();
    }
}
