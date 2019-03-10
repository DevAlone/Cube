using UnityEngine;

public abstract class BonusBase : MonoBehaviour
{
    // implement this method
    public abstract void ApplyBonus();
    public GameObject visualObject;

    protected GameController gameController;

    protected virtual void Start()
    {
        gameController = GameController.GetCurrent();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            ApplyBonus();
            Destroy(visualObject);
        }
    }
}
