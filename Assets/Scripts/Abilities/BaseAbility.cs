using System.Collections;
using UnityEngine;

public abstract class BaseAbility : MonoBehaviour
{
    public bool autoActivated = true;
    public int pointsToActivate;
    public int durationSeconds;
    public int Points
    {
        get { return _points; }
        set
        {
            _points += value;
            if (autoActivated)
            {
                TryToActivate();
            }
        }
    }
    public bool CanBeActivated
    {
        get { return _points >= pointsToActivate; }
    }

    public void TryToActivate()
    {
        if (_points >= pointsToActivate)
        {
            StartCoroutine(Activate());
            _points = 0;
        }
    }

    protected abstract IEnumerator Activate();
    protected GameController gameController;
    private int _points;

    private void Start()
    {
        gameController = GameController.GetCurrent();
    }
}
