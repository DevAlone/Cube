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

    // Does BFS search to find any path to row with an index rowIndex
    public List<Vector2Int> FindPathToRow(Vector2Int startPosition, int targetRowIndex)
    {
        var path = new List<Vector2Int>();
        _FindPathToRow(startPosition, targetRowIndex, path, new HashSet<Vector2Int>());
        path.Reverse();
        return path;
    }

    // is it a good idea to move to this cell
    private bool isCellWalkable(Vector2Int position)
    {
        // skip ones that are out of the array
        if (position.x < 0 || position.x >= columns || position.y < 0 || position.y >= rows)
        {
            return false;
        }
        var groundObj = objectsMap[0][position.y][position.x];
        // skip holes in the ground
        if (groundObj == null)
        {
            return false;
        }
        // skip hazards in the ground
        if (groundObj.tag == "Hazard")
        {
            return false;
        }
        // skip hazards above the ground
        var obj = objectsMap[1][position.y][position.x];
        if (obj != null && obj.tag == "Hazard")
        {
            return false;
        }

        return true;
    }

    private bool _FindPathToRow(Vector2Int currentPosition, int targetRowIndex, List<Vector2Int> path, HashSet<Vector2Int> visitedNodes)
    {
        if (currentPosition.y == targetRowIndex)
        {
            // found some path
            return true;
        }

        if (visitedNodes.Contains(currentPosition))
        {
            return false;
        }
        visitedNodes.Add(currentPosition);

        if (!isCellWalkable(currentPosition))
        {
            return false;
        }

        if (_FindPathToRow(
            new Vector2Int(currentPosition.x, currentPosition.y + 1),
            targetRowIndex,
            path,
            visitedNodes
        ))
        {
            path.Add(currentPosition);
            return true;
        }
        if (_FindPathToRow(
            new Vector2Int(currentPosition.x - 1, currentPosition.y),
            targetRowIndex,
            path,
            visitedNodes
        ))
        {
            path.Add(currentPosition);
            return true;
        }
        if (_FindPathToRow(
           new Vector2Int(currentPosition.x + 1, currentPosition.y),
           targetRowIndex,
           path,
           visitedNodes
       ))
        {
            path.Add(currentPosition);
            return true;
        }
        if (_FindPathToRow(
            new Vector2Int(currentPosition.x, currentPosition.y - 1),
            targetRowIndex,
            path,
            visitedNodes
        ))
        {
            path.Add(currentPosition);
            return true;
        }

        return false;
    }

    public List<int> FindEntriesToRow(int rowIndex, Vector2Int playerPosition, HashSet<Vector2Int> visitedIndices)
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

        if (!isCellWalkable(playerPosition))
        {
            return new List<int>();
        }

        var result = new List<int>();

        result.AddRange(FindEntriesToRow(rowIndex, new Vector2Int(playerPosition.x - 1, playerPosition.y), visitedIndices));
        result.AddRange(FindEntriesToRow(rowIndex, new Vector2Int(playerPosition.x + 1, playerPosition.y), visitedIndices));
        result.AddRange(FindEntriesToRow(rowIndex, new Vector2Int(playerPosition.x, playerPosition.y + 1), visitedIndices));

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
