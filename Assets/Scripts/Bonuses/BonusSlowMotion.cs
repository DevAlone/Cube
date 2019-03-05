public class BonusSlowMotion : BonusBase
{
    public float speedModifier;
    private GameController gameController;

    public override void ApplyBonus()
    {
        if (gameController == null)
        {
            gameController = GameController.GetCurrent();
        }

        gameController.field.VerticalSpeed -= speedModifier;
    }
}
