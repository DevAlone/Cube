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
    // minimum amount of pixels considered as swipe
    public float swipeThreshold;
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
    private Vector3 touchStartPosition;

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

        // keyboard input
        foreach (var pair in keyInputActionMap)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                TryToPutAction(pair.Value);
            }
        }

        // sensor input
        if (Input.touches.Length > 0)
        {
            var touch = Input.touches[0];
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    break;
                case TouchPhase.Moved:
                    break;
                case TouchPhase.Ended:
                    float horizontalLength = touch.position.x - touchStartPosition.x;
                    // float verticalLength = touch.position.y - touchStartPosition.y;
                    if (Mathf.Abs(horizontalLength) > swipeThreshold)
                    {
                        TryToPutAction(horizontalLength > 0 ?
                                InputAction.MoveRight :
                                InputAction.MoveLeft);
                    }
                    else
                    {
                        TryToPutAction(InputAction.SkipStep);
                    }
                    /*
                    if (Mathf.Abs(touch.position.x - touchStartPosition.x) <= swipeThreshold)
                    {
                        TryToPutAction(InputAction.SkipStep);
                    }
                    touchStartPosition = touch.position;
					*/
                    break;
            }
        }
    }

    void TryToPutAction(InputAction action)
    {
        if (inputQueue.Count > 0)
        {
            var lastAction = inputQueue.Last.Value;
            if (AreActionsOpposite(lastAction, action))
            {
                onLastActionRemoved?.Invoke(lastAction);
                inputQueue.RemoveLast();
                return;
            }
        }

        if (inputQueue.Count < maximumQueueSize)
        {
            inputQueue.AddLast(action);
            onActionAdded?.Invoke(action);
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
