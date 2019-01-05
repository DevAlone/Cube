using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputAction
{
    MoveLeft,
    MoveRight,
    SkipStep,
}

public class InputQueue : MonoBehaviour
{
    public delegate void OnActionAdded(InputAction action);
    public delegate void OnActionRemoved(InputAction action);
    public OnActionAdded onActionAdded;
    public OnActionRemoved onActionRemoved;

    private Queue<InputAction> inputQueue;
    private Dictionary<KeyCode, InputAction> keyInputActionMap;
    private Dictionary<InputAction, InputAction> oppositeActionsMap;

    public bool IsEmpty()
    {
        return inputQueue != null && inputQueue.Count == 0;
    }

    public InputAction Dequeue()
    {
        onActionAdded?.Invoke(inputQueue.Peek());
        return inputQueue.Dequeue();
    }

    public InputAction Peek()
    {
        return inputQueue.Peek();
    }

    void Start()
    {
        inputQueue = new Queue<InputAction>();
        keyInputActionMap = new Dictionary<KeyCode, InputAction>
        {
            { KeyCode.LeftArrow, InputAction.MoveLeft },
            { KeyCode.RightArrow, InputAction.MoveRight },
            { KeyCode.UpArrow, InputAction.SkipStep },
        };
        oppositeActionsMap = new Dictionary<InputAction, InputAction>
        {
            { InputAction.MoveLeft, InputAction.MoveRight },
            { InputAction.MoveRight, InputAction.MoveLeft },
        };
    }

    void Update()
    {
        foreach (var pair in keyInputActionMap)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                if (inputQueue.Count > 0)
                {
                    var lastAction = inputQueue.Peek();
                    if (AreActionsOpposite(lastAction, pair.Value))
                    {
                        onActionRemoved(inputQueue.Dequeue());
                        continue;
                    }
                }

                inputQueue.Enqueue(pair.Value);
                onActionAdded?.Invoke(pair.Value);
            }
        }
    }

    bool AreActionsOpposite(InputAction action1, InputAction action2)
    {
        InputAction oppositeToAction1;
        if (oppositeActionsMap.TryGetValue(action1, out oppositeToAction1))
        {
            return oppositeToAction1 == action2;
        }

        return false;
    }
}
