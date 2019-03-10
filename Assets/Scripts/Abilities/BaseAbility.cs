using System.Collections;
using UnityEngine;

public abstract class BaseAbility : MonoBehaviour
{
    public int pointsToActivate;
    public int Points
    {
        get { return _points; }
        set
        {
            _points += value;
        }
    }
    public virtual bool CanBeActivated
    {
        get { return _points >= pointsToActivate; }
    }

    public void TryToActivate()
    {
        if (CanBeActivated)
        {
            StartCoroutine(Activate());
            _points = 0;
        }
    }

    protected abstract IEnumerator Activate();
    protected GameController gameController;
    private int _points;

    protected virtual void Start()
    {
        gameController = GameController.GetCurrent();
    }
}
