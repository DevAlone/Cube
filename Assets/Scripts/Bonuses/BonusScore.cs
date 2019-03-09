public class BonusScore : BonusBase
{
    public int score;
    public override void ApplyBonus()
    {
        gameController.Score += score;
    }
}
