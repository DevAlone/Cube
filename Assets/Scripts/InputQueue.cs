using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputAction
{
    MoveLeft,
    MoveRight,
    SkipStep,
    UndoSkipStep,
}

public class InputQueue : MonoBehaviour, IEnumerable<InputAction>
{
    public Field field;
    // minimum amount of pixels considered as swipe
    public float swipeHorizontalThreshold;
    public float swipeVerticalThreshold;
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

    public void Clear()
    {
        // inputQueue.Clear();
        while (inputQueue.Count > 0)
        {
            inputQueue.RemoveFirst();
        }
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
            { KeyCode.DownArrow, InputAction.UndoSkipStep },
        };
        oppositeActionsMap = new Dictionary<InputAction, InputAction>
        {
            { InputAction.MoveLeft, InputAction.MoveRight },
            { InputAction.MoveRight, InputAction.MoveLeft },
            { InputAction.UndoSkipStep, InputAction.SkipStep},
            { InputAction.SkipStep, InputAction.UndoSkipStep},
        };
    }

    void Update()
    {
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
                case TouchPhase.Ended:
                    float horizontalLength = touch.position.x - touchStartPosition.x;
                    float verticalLength = touch.position.y - touchStartPosition.y;
                    bool swipeDetected = false;

                    if (Mathf.Abs(verticalLength) > swipeVerticalThreshold)
                    {
                        swipeDetected = true;
                        TryToPutAction(verticalLength > 0 ?
                                InputAction.SkipStep :
                                InputAction.UndoSkipStep);
                    }
                    if (Mathf.Abs(horizontalLength) > swipeHorizontalThreshold)
                    {
                        swipeDetected = true;
                        TryToPutAction(horizontalLength > 0 ?
                                InputAction.MoveRight :
                                InputAction.MoveLeft);
                    }
                    if (!swipeDetected)
                    {
                        TryToPutAction(InputAction.SkipStep);
                    }
                    break;
            }
        }
    }

    public void TryToPutAction(InputAction action)
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
        if (inputQueue.Count >= maximumQueueSize)
        {
            return;
        }
        if (action == InputAction.UndoSkipStep)
        {
            return;
        }
        // limit to field 
        if (action == InputAction.MoveLeft && field.currentTargetPosition.x <= 0 ||
            action == InputAction.MoveRight && field.currentTargetPosition.x >= field.columns - 1 ||
            action == InputAction.SkipStep && field.currentTargetPosition.y >= field.rows - 1)
        {
            return;
        }

        inputQueue.AddLast(action);
        onActionAdded?.Invoke(action);
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
