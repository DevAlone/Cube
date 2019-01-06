using System.Collections;
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
    // number of cells in the field
    public int width, height;
    public float cellSize;
    public float verticalSpeed;
    // per row
    public float verticalAcceleration;
    public float maximumVerticalSpeed;
    public float hazardProbability;
    public float hazardProbabilityIncrementer;
    public float maximumHazardProbability;
    public float hazardInTheGroundProbability;
    public float hazardInTheGroundProbabilityIncrementer;
    public float maximumHazardInTheGroundProbability;
    public GameObject[] groundObjectPrefabs;
    public GameObject[] groundHazardObjectPrefabs;
    public GameObject[] hazardObjectPrefabs;
    public delegate void OnRowCreatedDelegate();
    public OnRowCreatedDelegate onRowCreated;
    public GameObject player;
    public InputQueue inputQueue;
    public Material targetCellMaterial;
    public BonusEntry[] bonuses;

    private CircularBuffer<GameObject[]>[] objectsMap;
    private Vector2Int playerPositionInMap;
    private Vector2Int currentTargetPosition;
    private const int numberOfLayers = 2;
    private PlayerController playerController;
    private GameController gameController;

    public void Move(float deltaTime)
    {
        int len = height;

        for (int z = 0; z < len; ++z)
        {
            bool wasRowDeleted = false;
            float deletedObjZPosition = 0;

            for (int layer = 0; layer < numberOfLayers; ++layer)
            {

                for (int x = 0; x < width; ++x)
                {
                    var obj = objectsMap[layer][z][x];

                    if (obj != null)
                    {
                        obj.transform.position -= new Vector3(0, 0, verticalSpeed * deltaTime * gameController.gameSpeedModifier);
                        if (obj.transform.position.z <= 0)
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
                    height - 1,
                    new Vector3(-(width / 2) * cellSize, 0.0f, deletedObjZPosition + (height - 1) * cellSize)
                );
            }
        }
    }

    void Start()
    {
        gameController = GameController.GetCurrent();

        objectsMap = new CircularBuffer<GameObject[]>[2];
        for (int layer = 0; layer < 2; ++layer)
        {
            objectsMap[layer] = new CircularBuffer<GameObject[]>(height);
        }

        for (int z = 0; z < height; ++z)
        {
            for (int layer = 0; layer < 2; ++layer)
            {
                objectsMap[layer][z] = new GameObject[width];
            }
        }

        for (int z = 0; z < height; ++z)
        {
            CreateRow(z, new Vector3(-(width / 2) * cellSize, 0, z * cellSize), false);
        }

        playerController = player.GetComponent<ActualObjectHolder>().actualObject.GetComponent<PlayerController>();

        onRowCreated += () =>
        {
            verticalSpeed += verticalAcceleration;
            if (verticalSpeed > maximumVerticalSpeed)
            {
                verticalSpeed = maximumVerticalSpeed;
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
            currentTargetPosition.y -= 1;
            if (currentTargetPosition.y < playerPositionInMap.y)
            {
                currentTargetPosition.y = playerPositionInMap.y;
            }

            UpdatePlayerPosition(Vector2Int.zero);
        };

        playerController.onStartedMovingHorizontally += (float direction) =>
        {
            UpdatePlayerPosition(new Vector2Int((int)direction, 0));
        };

        inputQueue.onActionAdded += (InputAction action) =>
        {
            /*
            MoveCurrentTargetPosition(action, true);
            try
            {
                var groundObject = groundGameObjectsMap[currentTargetPosition.y][currentTargetPosition.x];
                if (groundObject != null)
                {
                    groundObject.GetComponent<ActualObjectHolder>().actualObject.GetComponent<MeshRenderer>().material = targetCellMaterial;
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
                Debug.Log(currentTargetPosition);
            }
*/
        };

        inputQueue.onActionRemoved += (InputAction action) =>
        {
            // MoveCurrentTargetPosition(action, false);
        };
    }

    void MoveCurrentTargetPosition(InputAction action, bool wasAdded)
    {
        /*
        if (currentTargetPosition == null)
        {
            currentTargetPosition = playerPositionInMap;
        }

        switch (action)
        {
            case InputAction.MoveLeft:
                currentTargetPosition = new Vector2Int(currentTargetPosition.x + (wasAdded ? 1 : -1), currentTargetPosition.y);
                break;
            case InputAction.MoveRight:
                currentTargetPosition = new Vector2Int(currentTargetPosition.x - (wasAdded ? 1 : -1), currentTargetPosition.y);
                break;
            case InputAction.SkipStep:
                currentTargetPosition = new Vector2Int(currentTargetPosition.x, currentTargetPosition.y + (wasAdded ? 1 : -1));
                break;
            default:
                throw new System.Exception();
        }
        */
    }

    void UpdatePlayerPosition(Vector2Int direction)
    {
        // TODO: fix position detection!

        // field is moving, so we need to keep player's position fresh
        playerPositionInMap = new Vector2Int(
            (int)((player.transform.position.x + cellSize / 2) / cellSize + width / 2),
            (int)((player.transform.position.z - cellSize / 2) / cellSize)
        );
        if (playerPositionInMap.x < 0)
        {
            playerPositionInMap.x = 0;
        }
        if (playerPositionInMap.x >= width)
        {
            playerPositionInMap.x = width - 1;
        }

        if (playerPositionInMap.y < 0)
        {
            playerPositionInMap.y = 0;
        }
        if (playerPositionInMap.y >= height)
        {
            playerPositionInMap.y = height - 1;
        }

        MarkCellAsTarget(playerPositionInMap);
        MarkCellAsTarget(playerPositionInMap + new Vector2Int(
            direction.x == 0 ? 0 : direction.x > 0 ? 1 : -1,
            direction.y == 0 ? 0 : direction.y > 0 ? 1 : -1
        ));
    }

    void MarkCellAsTarget(Vector2Int position)
    {
        if (position.x < 0 || position.x >= width ||
            position.y < 0 || position.y >= height)
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


    void Update()
    {
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

        for (int i = 0; i < width; ++i)
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
                if (bonuses.Length> 0)
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

        if (playerPosition.x < 0 || playerPosition.x >= width || playerPosition.y < 0 || playerPosition.y >= height)
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
