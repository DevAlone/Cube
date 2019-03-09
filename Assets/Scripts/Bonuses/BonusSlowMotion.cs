public class BonusSlowMotion : BonusChangeSpeed
{
    public int abilityPointsIncrementer = 1;

    public override void ApplyBonus()
    {
        base.ApplyBonus();
        var slowMoAbility = gameController.GetComponent<SlowMotionAbility>();
        slowMoAbility.Points += abilityPointsIncrementer;
    }
}
