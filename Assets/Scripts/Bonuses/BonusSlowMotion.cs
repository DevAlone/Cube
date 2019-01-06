using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusSlowMotion : BonusBase
{
    // TODO: fix multiple bonuses attached
    public float durationSeconds;
    public float speedModifier;

    private GameController gameController;

    public override void ApplyBonus()
    {
        StartCoroutine(DoSlowMo());
    }
    IEnumerator DoSlowMo()
    {
        if (gameController == null)
        {
            gameController = GameController.GetCurrent();
        }

        gameController.gameSpeedModifier = speedModifier;
        yield return new WaitForSeconds(durationSeconds);
        gameController.ResetGameSpeedModifier();
    }
}
