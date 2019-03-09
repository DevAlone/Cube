using UnityEngine;

public class BonusChangeSpeed : BonusBase
{
    public float speedModifier;
    private PlayerController playerController;

    public override void ApplyBonus()
    {
        if (playerController == null)
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        gameController.field.verticalSpeed += speedModifier;
        playerController.HorizontalSpeed += speedModifier;
    }
}
