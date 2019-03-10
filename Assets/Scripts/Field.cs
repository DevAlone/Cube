using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BonusEntry
{
    public GameObject prefab;
    public float probability;
}

public class Field : MonoBehaviour
{
    public GameObject player;
    public InputQueue inputQueue;
    // value from 0.0f to 1.0f
    public float targetCellBrightness;

    // number of cells in the field
    public int columns, rows;
    public float cellSize;
    public LimitedFloatValue verticalSpeed;

    // per row
    public float verticalAcceleration;
    public delegate void OnRowCreatedDelegate();
    public OnRowCreatedDelegate onRowCreated;

    public CircularBuffer<GameObject[]>[] objectsMap;
    public Vector2Int playerPositionInMap;
    public Vector2Int currentTargetPosition;
    // first is the groud, second is where cube, hazards and bonuses live
    private const int numberOfLayers = 2;
    private PlayerController playerController;
    private GameController gameController;
    private FieldGenerator fieldGenerator;

    public void Move(float deltaTime)
    {
        int len = rows;

        for (int z = 0; z < len; ++z)
        {
            bool wasRowDeleted = false;
            float deletedObjZPosition = 0;

            for (int layer = 0; layer < numberOfLayers; ++layer)
            {
                for (int x = 0; x < columns; ++x)
                {
                    var obj = objectsMap[layer][z][x];

                    if (obj != null)
                    {
                        obj.transform.position -= new Vector3(0, 0, verticalSpeed * deltaTime * gameController.gameSpeedModifier);
                        if (obj.transform.position.z < 0)
                        {
                            deletedObjZPosition = obj.transform.position.z;
                            wasRowDeleted = true;
                            Destroy(obj);
                            objectsMap[layer][z][x] = null;
                        }
                    }
                }
            }

            if (wasRowDeleted)
            {
                for (int layer = 0; layer < numberOfLayers; ++layer)
                {
                    objectsMap[layer].Rotate(1);
                }
                currentTargetPosition.y -= 1;
                --len;
                --z;
                UpdatePlayerPosition();

                fieldGenerator.CreateRow(
                    this,
                    rows - 1,
                    // position of the last row
                    new Vector3(
                        -(columns / 2) * cellSize,
                        0.0f,
                        deletedObjZPosition + rows * cellSize
                    )
                );
            }
        }
    }

    public void MarkCellAsTarget(Vector2Int position)
    {
        if (position.x < 0 || position.x >= columns ||
            position.y < 0 || position.y >= rows)
        {
            return;
        }

        var groundObj = objectsMap[0][position.y][position.x];

        if (groundObj != null)
        {
            var mt = groundObj.
                GetComponent<ActualObjectHolder>().
                actualObject.
                GetComponent<MeshRenderer>().
                material;  // = targetCellMaterial;
            var color = mt.color;
            color.r += targetCellBrightness;
            color.g += targetCellBrightness;
            color.b += targetCellBrightness;
            mt.color = color;
        }
    }

    public void UnmarkCellAsTarget(Vector2Int position)
    {
        if (position.x < 0 || position.x >= columns ||
            position.y < 0 || position.y >= rows)
        {
            return;
        }

        var groundObj = objectsMap[0][position.y][position.x];

        if (groundObj != null)
        {
            var mt = groundObj.
                GetComponent<ActualObjectHolder>().
                actualObject.
                GetComponent<MeshRenderer>().
                material;  // = targetCellMaterial;
            var color = mt.color;
            color.r -= targetCellBrightness;
            color.g -= targetCellBrightness;
            color.b -= targetCellBrightness;
            mt.color = color;
        }
    }

    public List<int> FindPathsToRow(int rowIndex, Vector2Int playerPosition, HashSet<Vector2Int> visitedIndices)
    {
        if (visitedIndices.Contains(playerPosition))
        {
            return new List<int>();
        }
        visitedIndices.Add(playerPosition);

        if (playerPosition.x < 0 || playerPosition.x >= columns || playerPosition.y < 0 || playerPosition.y >= rows)
        {
            return new List<int>();
        }

        if (rowIndex == playerPosition.y)
        {
            return new List<int> {
                playerPosition.x
            };
        }

        // skip holes in the ground
        if (objectsMap[0][playerPosition.y][playerPosition.x] == null)
        {
            return new List<int>();
        }

        // skip hazards in the ground
        if (objectsMap[0][playerPosition.y][playerPosition.x].tag == "Hazard")
        {
            return new List<int>();
        }

        // skip hazards above the ground
        if (objectsMap[1][playerPosition.y][playerPosition.x] != null &&
            objectsMap[1][playerPosition.y][playerPosition.x].tag == "Hazard")
        {
            return new List<int>();
        }

        var result = new List<int>();

        result.AddRange(FindPathsToRow(rowIndex, new Vector2Int(playerPosition.x - 1, playerPosition.y), visitedIndices));
        result.AddRange(FindPathsToRow(rowIndex, new Vector2Int(playerPosition.x + 1, playerPosition.y), visitedIndices));
        result.AddRange(FindPathsToRow(rowIndex, new Vector2Int(playerPosition.x, playerPosition.y + 1), visitedIndices));

        return result;
    }

    GameObject findFirstGroundObject()
    {
        for (int z = 0; z < rows; ++z)
        {
            for (int x = 0; x < columns; ++x)
            {
                var obj = objectsMap[0][z][x];
                if (obj != null)
                {
                    return obj;
                }
            }
        }
        throw new System.Exception("unable to find first object");
    }
    public void UpdatePlayerPosition()
    {
        var firstObject = findFirstGroundObject();
        var fieldStartPosition = firstObject.transform.position.z - firstObject.transform.localScale.y / 2;

        playerPositionInMap = new Vector2Int(
            (int)((player.transform.position.x + columns * cellSize / 2) / cellSize),
            (int)((player.transform.position.z - fieldStartPosition) / cellSize)
        );
    }

    public void DrawPath()
    {
        UpdatePlayerPosition();
        for (int z = 0; z < rows && z < inputQueue.Size; ++z)
        {
            for (int x = 0; x < columns; ++x)
            {
                UnmarkCellAsTarget(new Vector2Int(x, z));
            }
        }

        // draw path
        MarkCellAsTarget(playerPositionInMap);
        currentTargetPosition = playerPositionInMap;
        foreach (var action in inputQueue)
        {
            switch (action)
            {
                case InputAction.MoveLeft:
                    currentTargetPosition.x -= 1;
                    break;
                case InputAction.MoveRight:
                    currentTargetPosition.x += 1;
                    break;
                case InputAction.SkipStep:
                    currentTargetPosition.y += 1;
                    break;
            }
            MarkCellAsTarget(currentTargetPosition);
        }
        if (inputQueue.Size <= 0)
        {
            currentTargetPosition.y += 1;
            MarkCellAsTarget(currentTargetPosition);
        }
    }

    void Start()
    {
        fieldGenerator = GetComponent<FieldGenerator>();
        gameController = GameController.GetCurrent();

        objectsMap = new CircularBuffer<GameObject[]>[numberOfLayers];
        for (int layer = 0; layer < numberOfLayers; ++layer)
        {
            objectsMap[layer] = new CircularBuffer<GameObject[]>(rows);
        }

        for (int layer = 0; layer < numberOfLayers; ++layer)
        {
            for (int z = 0; z < rows; ++z)
            {
                objectsMap[layer][z] = new GameObject[columns];
            }
        }

        for (int z = 0; z < rows; ++z)
        {
            fieldGenerator.CreateRow(this, z, new Vector3(-(columns / 2) * cellSize, 0, z * cellSize), false);
        }

        playerController = player.
            GetComponent<ActualObjectHolder>().
            actualObject.
            GetComponent<PlayerController>();

        UpdatePlayerPosition();
        currentTargetPosition = playerPositionInMap;
    }

    private void Update()
    {
        UpdatePlayerPosition();
    }

}
