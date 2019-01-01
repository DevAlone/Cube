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
    private Queue<InputAction> inputQueue;
    private Dictionary<KeyCode, InputAction> keyInputActionMap;

    public bool IsEmpty()
    {
        return inputQueue != null && inputQueue.Count == 0;
    }

    public InputAction Dequeue()
    {
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
    }

    void Update()
    {
        foreach (var pair in keyInputActionMap)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                inputQueue.Enqueue(pair.Value);
            }
        }
    }
}
