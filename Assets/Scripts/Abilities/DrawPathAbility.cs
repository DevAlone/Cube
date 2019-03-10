using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPathAbility : BaseAbility
{
    private Field field;
    private InputQueue inputQueue;

    protected override void Start()
    {
        base.Start();
        field = gameController.field;
        inputQueue = gameController.inputQueue;
    }

    protected override IEnumerator Activate()
    {
        var path = field.FindPathToRow(field.currentTargetPosition, field.rows - 1);


        for (int i = 1; i < path.Count; ++i)
        {
            var prevPosition = path[i - 1];
            var position = path[i];
            if (prevPosition.y < position.y)
            {
                inputQueue.TryToPutAction(InputAction.SkipStep);
            }
            else if (prevPosition.y > position.y)
            {
                inputQueue.TryToPutAction(InputAction.UndoSkipStep);
            }
            else if (prevPosition.x < position.x)
            {
                inputQueue.TryToPutAction(InputAction.MoveRight);
            }
            else if (prevPosition.x > position.x)
            {
                inputQueue.TryToPutAction(InputAction.MoveLeft);
            }
            else
            {
                Debug.Log("Wrong path. prev cell '" + prevPosition + "', cerr cell '" + position + "'");
            }
        }

        yield break;
    }
}
