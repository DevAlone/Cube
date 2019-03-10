using UnityEngine;
using UnityEngine.UI;

public class AbilitiesManager : MonoBehaviour
{
    public Button activateSlowMoAbilityButton;
    public Button activateSpeedUpAbilityButton;

    private GameController gameController;
    private SlowMotionAbility slowMoAbility;
    private SpeedUpAbility speedUpAbility;

    void Start()
    {
        gameController = GameController.GetCurrent();
        slowMoAbility = gameController.GetComponent<SlowMotionAbility>();
        speedUpAbility = gameController.GetComponent<SpeedUpAbility>();
        activateSlowMoAbilityButton.onClick.AddListener(() =>
        {
            slowMoAbility.TryToActivate();
        });

        activateSpeedUpAbilityButton.onClick.AddListener(() =>
        {
            speedUpAbility.TryToActivate();
        });
    }

    void Update()
    {
        activateSlowMoAbilityButton.gameObject.SetActive(
            slowMoAbility.CanBeActivated
        );
        activateSpeedUpAbilityButton.gameObject.SetActive(
            speedUpAbility.CanBeActivated
        );
    }
}
