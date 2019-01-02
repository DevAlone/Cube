using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    public int width, height;
    public float stepSize;
    public float verticalSpeed;
    // per row
    public float verticalAcceleration;
    public float maximumVerticalSpeed;
    public float hazardProbability;
    public float holeInTheGroundProbability;
    public GameObject[] groundObjectPrefabs;
    // public GameObject invisibleGroundObjectPrefab;
    public GameObject[] hazardObjectPrefabs;
    public delegate void OnRowCreatedDelegate();
    public OnRowCreatedDelegate onRowCreated;
    public GameObject player;

    private GameObject[,] groundGameObjectsMap;
    private GameObject[,] hazardGameObjectsMap;
    public int fieldStartRow = 0;

    public void Move()
    {
        for (int z = 0; z < height; ++z)
        {
            bool wasRowDeleted = false;
            for (int x = 0; x < width; ++x)
            {
                var groundGameObject = groundGameObjectsMap[z, x];
                var hazardGameObject = hazardGameObjectsMap[z, x];

                if (groundGameObject != null)
                {
                    groundGameObject.transform.position += -new Vector3(0, 0, verticalSpeed * Time.deltaTime);
                }
                if (hazardGameObject != null)
                {
                    hazardGameObject.transform.position += -new Vector3(0, 0, verticalSpeed * Time.deltaTime);
                }

                if (groundGameObject != null && groundGameObject.transform.position.z <= 0)
                {
                    wasRowDeleted = true;
                    Destroy(groundGameObject);
                    groundGameObjectsMap[z, x] = null;
                    if (hazardGameObject != null)
                    {
                        Destroy(hazardGameObject);
                        hazardGameObjectsMap[z, x] = null;
                    }
                }
            }

            if (wasRowDeleted)
            {
                fieldStartRow = Mod(fieldStartRow + 1, height);
                var lastRowIndex = Mod(z - 1, height);
                CreateRow(
                    z,
                    new Vector3(-(width / 2) * stepSize, 0, (height - 1) * stepSize) -
                    new Vector3(0, 0, verticalSpeed * Time.deltaTime)
                );
            }
        }
    }

    void Start()
    {
        groundGameObjectsMap = new GameObject[height, width];
        hazardGameObjectsMap = new GameObject[height, width];

        for (int z = 0; z < height; ++z)
        {
            CreateRow(z, new Vector3(-width / 2, 0, z * stepSize), false);
        }

        onRowCreated += () =>
        {
            verticalSpeed += verticalAcceleration;
            if (verticalSpeed > maximumVerticalSpeed)
            {
                verticalSpeed = maximumVerticalSpeed;
            }
        };
    }

    void Update()
    {

    }

    void CreateRow(int rowNumber, Vector3 shiftPosition, bool spawnHazards = true)
    {
        // TODO: detect player's position
        var paths = FindPathsToRow(rowNumber, new Vector2Int(0, fieldStartRow + 1), new HashSet<Vector2Int>());

        /*foreach (var item in paths)
        {
            Debug.Log(item);
        }
        Debug.Log("end");*/

        var skipIndex = paths.Count > 0 ? paths[Random.Range(0, paths.Count)] : -1;

        if (skipIndex >= 0)
        {
            var position = shiftPosition + new Vector3(
                skipIndex * stepSize,
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
            groundGameObjectsMap[rowNumber, skipIndex] = groundObj;
        }

        for (int i = 0; i < width; ++i)
        {
            if (skipIndex == i)
            {
                continue;
            }

            var position = shiftPosition + new Vector3(
                i * stepSize,
                0,
                0
            );

            var prefab = groundObjectPrefabs[Random.Range(0, groundObjectPrefabs.Length)];
            if (spawnHazards && Random.value < holeInTheGroundProbability)
            {
                // prefab = invisibleGroundObjectPrefab;
                continue;
            }

            var groundObj = Instantiate(
                prefab,
                position,
                transform.rotation
            );
            groundObj.transform.parent = transform;
            groundGameObjectsMap[rowNumber, i] = groundObj;

            if (spawnHazards && Random.value < hazardProbability)
            {
                var hazardPrefab = hazardObjectPrefabs[Random.Range(0, hazardObjectPrefabs.Length)];
                var hazard = Instantiate(
                    hazardPrefab,
                    groundObj.transform.position + new Vector3(0, groundObj.transform.localScale.y, 0),
                    groundObj.transform.rotation
                );
                hazard.transform.parent = transform;
                hazardGameObjectsMap[rowNumber, i] = hazard;
            }
        }

        onRowCreated?.Invoke();
    }

    List<int> FindPathsToRow(int rowIndex, Vector2Int playerPosition, HashSet<Vector2Int> visitedIndices)
    {
        playerPosition.y = Mod(playerPosition.y, height);

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

        if (groundGameObjectsMap[playerPosition.y, playerPosition.x] == null) 
            // || hazardGameObjectsMap[playerPosition.y, playerPosition.x] != null)
        {
            return new List<int>();
        }

        var result = new List<int>();

        result.AddRange(FindPathsToRow(rowIndex, new Vector2Int(playerPosition.x - 1, playerPosition.y), visitedIndices));
        result.AddRange(FindPathsToRow(rowIndex, new Vector2Int(playerPosition.x + 1, playerPosition.y), visitedIndices));
        result.AddRange(FindPathsToRow(rowIndex, new Vector2Int(playerPosition.x, playerPosition.y + 1), visitedIndices));
        // result.AddRange(FindPathsToRow(rowIndex, new Vector2Int(playerPosition.x, playerPosition.y - 1)));

        return result;
    }

    int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
