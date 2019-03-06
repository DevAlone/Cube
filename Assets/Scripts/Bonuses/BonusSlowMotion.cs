using UnityEngine;

public class BonusSlowMotion : BonusBase
{
    public float speedModifier;
    private GameController gameController;
    private PlayerController playerController;

    public override void ApplyBonus()
    {
        if (gameController == null)
        {
            gameController = GameController.GetCurrent();
        }
        if (playerController == null)
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        gameController.field.VerticalSpeed -= speedModifier;
        playerController.HorizontalSpeed -= speedModifier;
    }
}
