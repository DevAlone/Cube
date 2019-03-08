using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputAction
{
    MoveLeft,
    MoveRight,
    SkipStep,
}

public class InputQueue : MonoBehaviour, IEnumerable<InputAction>
{
    public uint maximumQueueSize;
    public delegate void OnActionAdded(InputAction action);
    public delegate void OnLastActionRemoved(InputAction action);
    public OnActionAdded onActionAdded;
    public OnLastActionRemoved onLastActionRemoved;
    public int Size
    {
        get { return inputQueue.Count; }
    }

    private LinkedList<InputAction> inputQueue;
    private Dictionary<KeyCode, InputAction> keyInputActionMap;
    private Dictionary<InputAction, InputAction> oppositeActionsMap;

    public bool IsEmpty()
    {
        return inputQueue != null && inputQueue.Count == 0;
    }

    public InputAction Dequeue()
    {
        var item = inputQueue.First.Value;
        inputQueue.RemoveFirst();
        return item;
    }

    private InputAction Peek()
    {
        return inputQueue.First.Value;
    }

    void Start()
    {
        inputQueue = new LinkedList<InputAction>();
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
        /*
        var queueString = inputQueue.Count.ToString() + ": ";
        foreach (var item in inputQueue)
        {
            switch (item)
            {
                case InputAction.MoveLeft:
                    queueString += "<";
                    break;
                case InputAction.MoveRight:
                    queueString += ">";
                    break;
                case InputAction.SkipStep:
                    queueString += "|";
                    break;
            }
        }
        Debug.Log(queueString);
		*/

        foreach (var pair in keyInputActionMap)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                if (inputQueue.Count > 0)
                {
                    var lastAction = inputQueue.Last.Value;
                    if (AreActionsOpposite(lastAction, pair.Value))
                    {
                        onLastActionRemoved?.Invoke(lastAction);
                        inputQueue.RemoveLast();
                        continue;
                    }
                }

                if (inputQueue.Count < maximumQueueSize)
                {
                    inputQueue.AddLast(pair.Value);
                    onActionAdded?.Invoke(pair.Value);
                }
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

    public IEnumerator<InputAction> GetEnumerator()
    {
        return ((IEnumerable<InputAction>)inputQueue).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<InputAction>)inputQueue).GetEnumerator();
    }
}
