using UnityEngine;
using UnityEngine.UI;

public class AbilitiesManager : MonoBehaviour
{
    public Button activateSlowMoAbilityButton;
    public Button activateSpeedUpAbilityButton;
    public Button activateDrawPathAbilityButton;

    private GameController gameController;
    private SlowMotionAbility slowMoAbility;
    private SpeedUpAbility speedUpAbility;
    private DrawPathAbility drawPathAbility;

    void Start()
    {
        gameController = GameController.GetCurrent();
        slowMoAbility = gameController.GetComponent<SlowMotionAbility>();
        speedUpAbility = gameController.GetComponent<SpeedUpAbility>();
        drawPathAbility = gameController.GetComponent<DrawPathAbility>();

        activateSlowMoAbilityButton.onClick.AddListener(() =>
        {
            slowMoAbility.TryToActivate();
        });
        activateSpeedUpAbilityButton.onClick.AddListener(() =>
        {
            speedUpAbility.TryToActivate();
        });
        activateDrawPathAbilityButton.onClick.AddListener(() =>
        {
            drawPathAbility.TryToActivate();
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
        activateDrawPathAbilityButton.gameObject.SetActive(
            drawPathAbility.CanBeActivated
        );
    }
}
