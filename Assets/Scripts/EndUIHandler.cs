using UnityEngine;
using UnityEngine.EventSystems;

public class EndUIHandler : MonoBehaviour, IPointerDownHandler
{
    private InputQueue inputQueue;

    void Start()
    {
        inputQueue = GameController.GetCurrent().inputQueue;
    }

    public void OnPointerDown(PointerEventData data)
    {
        inputQueue.TryToPutAction(InputAction.SkipStep);
    }
}
