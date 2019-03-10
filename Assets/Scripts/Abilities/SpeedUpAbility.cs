using System.Collections;
using UnityEngine;

public class SpeedUpAbility : BaseAbility
{
    public float speedModifier;
    public int pointsToDeactivate;

    private InputQueue inputQueue;

    public override bool CanBeActivated
    {
        get
        {
            return inputQueue.Size >= pointsToActivate;
        }
    }

    protected override void Start()
    {
        base.Start();
        inputQueue = gameController.inputQueue;
    }

    protected override IEnumerator Activate()
    {
        gameController.gameSpeedModifier = speedModifier;
        while (true)
        {
            if (inputQueue.Size <= pointsToDeactivate)
            {
                break;
            }

            yield return new WaitForSeconds(0.001f);
        }
        gameController.ResetGameSpeedModifier();
    }
}
