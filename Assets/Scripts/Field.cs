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

    private CircularBuffer<GameObject[]> groundGameObjectsMap;
    private CircularBuffer<GameObject[]> hazardGameObjectsMap;
    private Vector2Int playerPositionInMap;

    public void Move()
    { 
        int len = height;

        for (int z = 0; z < len; ++z)
        {
            playerPositionInMap = new Vector2Int(
                (int)((player.transform.position.x + stepSize / 2) / stepSize + width / 2),
                (int)(player.transform.position.z / stepSize)
            );

            bool wasRowDeleted = false;
            for (int x = 0; x < width; ++x)
            {
                var groundGameObject = groundGameObjectsMap[z][x];
                var hazardGameObject = hazardGameObjectsMap[z][x];

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
                    groundGameObjectsMap[z][x] = null;
                    if (hazardGameObject != null)
                    {
                        Destroy(hazardGameObject);
                        hazardGameObjectsMap[z][x] = null;
                    }
                }
            }

            if (wasRowDeleted)
            {
                groundGameObjectsMap.Rotate(1);
                hazardGameObjectsMap.Rotate(1);

                CreateRow(
                    height - 1,
                    new Vector3(-(width / 2) * stepSize, 0, (height - 1) * stepSize) -
                    new Vector3(0, 0, verticalSpeed * Time.deltaTime)
                );
            }
        }
    }

    void Start()
    {
        groundGameObjectsMap = new CircularBuffer<GameObject[]>(height);
        hazardGameObjectsMap = new CircularBuffer<GameObject[]>(height);

        for (int z = 0; z < height; ++z)
        {
            groundGameObjectsMap[z] = new GameObject[width];
            hazardGameObjectsMap[z] = new GameObject[width];
        }

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
        };
    }

    void Update()
    {

    }

    void CreateRow(int rowNumber, Vector3 shiftPosition, bool spawnHazards = true)
    {
        var paths = FindPathsToRow(rowNumber, playerPositionInMap, new HashSet<Vector2Int>());

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
            groundGameObjectsMap[rowNumber][skipIndex] = groundObj;
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
                i * stepSize,
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
            groundGameObjectsMap[rowNumber][i] = groundObj;

            if (spawnHazards && Random.value < hazardProbability)
            {
                var hazardPrefab = hazardObjectPrefabs[Random.Range(0, hazardObjectPrefabs.Length)];
                var hazard = Instantiate(
                    hazardPrefab,
                    groundObj.transform.position + new Vector3(0, groundObj.transform.localScale.y, 0),
                    groundObj.transform.rotation
                );
                hazard.transform.parent = transform;
                hazardGameObjectsMap[rowNumber][i] = hazard;
            }
        }

        onRowCreated?.Invoke();
    }

    List<int> FindPathsToRow(int rowIndex, Vector2Int playerPosition, HashSet<Vector2Int> visitedIndices)
    {
        // playerPosition.y = Mod(playerPosition.y, height);

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

        // skip holes
        if (groundGameObjectsMap[playerPosition.y][playerPosition.x] == null || 
            hazardGameObjectsMap[playerPosition.y][playerPosition.x] != null)
        {
            return new List<int>();
        }
        // skip hazards
        if (groundGameObjectsMap[playerPosition.y][playerPosition.x].tag == "Hazard")
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
}
