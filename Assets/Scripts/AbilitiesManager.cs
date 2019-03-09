using UnityEngine;
using UnityEngine.UI;

public class AbilitiesManager : MonoBehaviour
{
    public Button activateSlowMoAbilityButton;

    private GameController gameController;
    private SlowMotionAbility SlowMoAbility;

    void Start()
    {
        gameController = GameController.GetCurrent();
        SlowMoAbility = gameController.GetComponent<SlowMotionAbility>();
        activateSlowMoAbilityButton.onClick.AddListener(() =>
        {
            SlowMoAbility.TryToActivate();
        });
    }

    void Update()
    {
        activateSlowMoAbilityButton.gameObject.SetActive(
            SlowMoAbility.CanBeActivated
        );
    }
}
