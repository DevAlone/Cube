public class BonusSlowMotion : BonusChangeSpeed
{
    public int abilityPointsIncrementer = 1;
    private SlowMotionAbility slowMoAbility;

    protected override void Start()
    {
        base.Start();
        slowMoAbility = gameController.GetComponent<SlowMotionAbility>();
    }

    public override void ApplyBonus()
    {
        base.ApplyBonus();
        slowMoAbility.Points += abilityPointsIncrementer;
    }
}
