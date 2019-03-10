public class BonusScore : BonusBase
{
    public int abilityPointsIncrementer = 1;
    public int score;
    private DrawPathAbility drawPathAbility;

    protected override void Start()
    {
        base.Start();
        drawPathAbility = gameController.GetComponent<DrawPathAbility>();
    }

    public override void ApplyBonus()
    {
        gameController.Score += score;
        drawPathAbility.Points += abilityPointsIncrementer;
    }
}
