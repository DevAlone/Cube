using System.Collections;
using UnityEngine;

public class SlowMotionAbility : BaseAbility
{
    public float speedModifier;
    public int durationSeconds;

    protected override IEnumerator Activate()
    {
        gameController.gameSpeedModifier = speedModifier;
        yield return new WaitForSeconds(durationSeconds);
        gameController.ResetGameSpeedModifier();
    }
}
