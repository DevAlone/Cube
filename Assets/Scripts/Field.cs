using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BonusEntry
{
    public GameObject prefab;
    public float probability;
}

public class Field : MonoBehaviour
{
    public GameObject player;
    public GameObject[] groundObjectPrefabs;
    public GameObject[] groundHazardObjectPrefabs;
    public GameObject[] hazardObjectPrefabs;
    // number of cells in the field
    public int columns, rows;
    public float cellSize;
    public float VerticalSpeed
    {
        get { return _verticalSpeed; }
        set
        {
            _verticalSpeed = value;
            if (_verticalSpeed < minimumVerticalSpeed)
            {
                _verticalSpeed = minimumVerticalSpeed;
            }
        }
    }
    // per row
    public float verticalAcceleration;
    public float maximumVerticalSpeed;
    public float minimumVerticalSpeed;
    public float hazardProbability;
    public float hazardProbabilityIncrementer;
    public float maximumHazardProbability;
    public float hazardInTheGroundProbability;
    public float hazardInTheGroundProbabilityIncrementer;
    public float maximumHazardInTheGroundProbability;
    public delegate void OnRowCreatedDelegate();
    public OnRowCreatedDelegate onRowCreated;
    public InputQueue inputQueue;
    public Material targetCellMaterial;
    public Material cellMaterial;
    public BonusEntry[] bonuses;

    private float _verticalSpeed;
    private CircularBuffer<GameObject[]>[] objectsMap;
    public Vector2Int playerPositionInMap;
    public Vector2Int currentTargetPosition;
    // first is the groud, second is where cube, hazards and bonuses live
    private const int numberOfLayers = 2;
    private PlayerController playerController;
    private GameController gameController;

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
                        obj.transform.position -= new Vector3(0, 0, VerticalSpeed * deltaTime * gameController.gameSpeedModifier);
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
                --len;
                --z;


                CreateRow(
                    rows - 1,
                    // position of the last row
                    new Vector3(-(columns / 2) * cellSize, 0.0f, deletedObjZPosition + (rows - 1) * cellSize)
                );
            }
        }
    }

    void Start()
    {
        VerticalSpeed = minimumVerticalSpeed;
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
            CreateRow(z, new Vector3(-(columns / 2) * cellSize, 0, z * cellSize), false);
        }

        playerController = player.GetComponent<ActualObjectHolder>().actualObject.GetComponent<PlayerController>();

        UpdatePlayerPosition();
        currentTargetPosition = playerPositionInMap;

        onRowCreated += () =>
        {
            VerticalSpeed += verticalAcceleration;
            if (VerticalSpeed > maximumVerticalSpeed)
            {
                VerticalSpeed = maximumVerticalSpeed;
            }
            hazardProbability += hazardProbabilityIncrementer;
            if (hazardProbability > maximumHazardProbability)
            {
                hazardProbability = maximumHazardProbability;
            }
            hazardInTheGroundProbability += hazardInTheGroundProbabilityIncrementer;
            if (hazardInTheGroundProbability > maximumHazardInTheGroundProbability)
            {
                hazardInTheGroundProbability = maximumHazardInTheGroundProbability;
            }

            UpdatePlayerPosition();
            currentTargetPosition.y -= 1;
            if (currentTargetPosition.y <= playerPositionInMap.y)
            {
                currentTargetPosition = playerPositionInMap;
                MarkCellAsTarget(currentTargetPosition);
            }
        };

        // if we're moving automatically, not by player's input
        onRowCreated += () =>
        {
            if (inputQueue.IsEmpty())
            {
                currentTargetPosition.y = playerPositionInMap.y + 1;
                MarkCellAsTarget(currentTargetPosition);
            }
        };

        inputQueue.onActionAdded += (InputAction action) =>
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
        };

        inputQueue.onLastActionRemoved += (InputAction action) =>
        {
            UnmarkCellAsTarget(currentTargetPosition);
            switch (action)
            {
                case InputAction.MoveLeft:
                    currentTargetPosition.x += 1;
                    break;
                case InputAction.MoveRight:
                    currentTargetPosition.x -= 1;
                    break;
                case InputAction.SkipStep:
                    currentTargetPosition.y -= 1;
                    break;
            }
        };
    }

    private void Update()
    {
        UpdatePlayerPosition();
    }

    void UpdatePlayerPosition()
    {
        // TODO: find a better way
        var firstElementPosition = new Vector3(0, 0, 0);
        foreach (var item in objectsMap[0][0])
        {
            if (item != null)
            {
                firstElementPosition = item.transform.position - new Vector3(0, 0, item.transform.localScale.z / 2);
                break;
            }
        }

        playerPositionInMap = new Vector2Int(
            (int)((player.transform.position.x + columns * cellSize / 2) / cellSize),
            (int)((player.transform.position.z - firstElementPosition.z) / cellSize)
        );

        /* 
        if (playerPositionInMap.y < 0)
        {
            playerPositionInMap.y = 0;
        }
        if (playerPositionInMap.y >= rows)
        {
            playerPositionInMap.y = rows - 1;
        }
        */

        // MarkCellAsTarget(playerPositionInMap);
        /*
        MarkCellAsTarget(playerPositionInMap + new Vector2Int(
            direction.x == 0 ? 0 : direction.x > 0 ? 1 : -1,
            direction.y == 0 ? 0 : direction.y > 0 ? 1 : -1
        ));
        */
    }

    void MarkCellAsTarget(Vector2Int position)
    {
        if (position.x < 0 || position.x >= columns ||
            position.y < 0 || position.y >= rows)
        {
            return;
        }

        var groundObj = objectsMap[0][position.y][position.x];

        if (groundObj != null)
        {
            groundObj.
                GetComponent<ActualObjectHolder>().
                actualObject.
                GetComponent<MeshRenderer>().
                material = targetCellMaterial;
        }
    }

    void UnmarkCellAsTarget(Vector2Int position)
    {
        if (position.x < 0 || position.x >= columns ||
            position.y < 0 || position.y >= rows)
        {
            return;
        }

        var groundObj = objectsMap[0][position.y][position.x];

        if (groundObj != null)
        {
            groundObj.
                GetComponent<ActualObjectHolder>().
                actualObject.
                GetComponent<MeshRenderer>().
                material = cellMaterial;
        }
    }


    void CreateRow(int rowNumber, Vector3 shiftPosition, bool spawnHazards = true)
    {
        var paths = FindPathsToRow(rowNumber, playerPositionInMap, new HashSet<Vector2Int>());

        var skipIndex = paths.Count > 0 ? paths[Random.Range(0, paths.Count)] : -1;

        if (skipIndex >= 0)
        {
            var position = shiftPosition + new Vector3(
                skipIndex * cellSize,
                0,
                0
            );
            var prefab = groundObjectPrefabs[Random.Range(0, groundObjectPrefabs.Length)];
            var groundObj = Instantiate(
                prefab,
                position,
                transform.rotation
            );
            groundObj.transform.parent = transform;
            if (objectsMap[0][rowNumber][skipIndex] != null)
            {
                throw new System.Exception("trying to override an existing object");
            }
            objectsMap[0][rowNumber][skipIndex] = groundObj;
        }
        else
        {
            Debug.Log("Unable to find path");
        }

        for (int i = 0; i < columns; ++i)
        {
            if (skipIndex == i)
            {
                continue;
            }

            var position = shiftPosition + new Vector3(
                i * cellSize,
                0,
                0
            );

            var prefab = groundObjectPrefabs[Random.Range(0, groundObjectPrefabs.Length)];
            if (spawnHazards && groundHazardObjectPrefabs.Length > 0 && Random.value < hazardInTheGroundProbability)
            {
                prefab = groundHazardObjectPrefabs[Random.Range(0, groundHazardObjectPrefabs.Length)];
            }

            var groundObj = Instantiate(
                prefab,
                position,
                transform.rotation
            );
            groundObj.transform.parent = transform;
            if (objectsMap[0][rowNumber][i] != null)
            {
                throw new System.Exception("trying to override an existing object");
            }
            objectsMap[0][rowNumber][i] = groundObj;

            if (spawnHazards && Random.value < hazardProbability)
            {
                var hazardPrefab = hazardObjectPrefabs[Random.Range(0, hazardObjectPrefabs.Length)];
                var hazard = Instantiate(
                    hazardPrefab,
                    groundObj.transform.position + new Vector3(0, groundObj.transform.localScale.y, 0),
                    groundObj.transform.rotation
                );
                hazard.transform.parent = transform;
                if (objectsMap[1][rowNumber][i] != null)
                {
                    throw new System.Exception("trying to override an existing object");
                }
                objectsMap[1][rowNumber][i] = hazard;
            }
            else
            {
                // try to spawn a bonus
                if (bonuses.Length > 0)
                {
                    var randomBonusEntry = bonuses[Random.Range(0, bonuses.Length)];
                    if (Random.value < randomBonusEntry.probability)
                    {
                        var bonus = Instantiate(
                            randomBonusEntry.prefab,
                            groundObj.transform.position + new Vector3(0, groundObj.transform.localScale.y, 0),
                            groundObj.transform.rotation
                        );
                        bonus.transform.parent = transform;
                        if (objectsMap[1][rowNumber][i] != null)
                        {
                            throw new System.Exception("trying to override an existing object");
                        }
                        objectsMap[1][rowNumber][i] = bonus;

                    }
                }
            }
        }

        onRowCreated?.Invoke();
    }

    List<int> FindPathsToRow(int rowIndex, Vector2Int playerPosition, HashSet<Vector2Int> visitedIndices)
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
}
