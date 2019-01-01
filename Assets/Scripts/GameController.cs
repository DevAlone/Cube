using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public int width, height;
    public float stepSize;
    public float fieldSpeed;
    public float playerHorizontalSpeed;
    public GameObject[] groundObjectPrefabs;
    public GameObject[] hazardObjectPrefabs;
    public GameObject playerObject;
    public InputQueue inputQueue;
    public bool gameIsOver = false;

    private GameObject[,] groundGameObjectsMap;
    private GameObject[,] hazardGameObjectsMap;
    private bool isProcessingInputQueue = false;
    private bool isMovingPlayer = false;
    private Vector3 playerTarget;

    void Start()
    {
        groundGameObjectsMap = new GameObject[width, height];
        hazardGameObjectsMap = new GameObject[width, height];

        for (int z = 0; z < height; ++z)
        {
            for (int x = 0; x < width; ++x)
            {
                var position = transform.position + new Vector3(
                    (x - width / 2) * stepSize,
                    0,
                    z * stepSize
                );
                // TODO: refactor
                var obj = Instantiate(groundObjectPrefabs[0], position, transform.rotation);
                groundGameObjectsMap[x, z] = obj;
                obj.transform.parent = transform;
            }
        }
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    void Update()
    {
        if (isProcessingInputQueue)
        {
            if (isMovingPlayer)
            {
                playerObject.transform.position = Vector3.MoveTowards(
                    playerObject.transform.position,
                    playerTarget,
                    playerHorizontalSpeed * Time.deltaTime
                );
                isMovingPlayer = playerObject.transform.position != playerTarget;
            }
            else if(inputQueue.IsEmpty())
            {
                isProcessingInputQueue = false;
            }
            else
            {
                
                var action = inputQueue.Dequeue();

                var direction = 0;
                switch (action)
                {
                    case InputAction.MoveLeft:
                        direction = -1;
                        break;
                    case InputAction.MoveRight:
                        direction = 1;
                        break;
                    default:
                        isProcessingInputQueue = false;
                        return;
                }

                playerTarget = playerObject.transform.position + new Vector3(direction * stepSize, 0, 0);

                isMovingPlayer = true;
            }
        }
        else
        {
            for (int z = 0; z < height; ++z)
            {
                bool wasRowDeleted = false;
                for (int x = 0; x < width; ++x)
                {
                    var groundGameObject = groundGameObjectsMap[x, z];
                    var hazardGameObject = hazardGameObjectsMap[x, z];

                    groundGameObject.transform.position += -new Vector3(0, 0, fieldSpeed * Time.deltaTime);
                    if (hazardGameObject != null)
                    {
                        hazardGameObject.transform.position += -new Vector3(0, 0, fieldSpeed * Time.deltaTime);
                    }
                    if (groundGameObject.transform.position.z <= 0)
                    {
                        wasRowDeleted = true;
                        Destroy(groundGameObject);
                        groundGameObjectsMap[x, z] = null;
                        if (hazardGameObject != null)
                        {
                            Destroy(hazardGameObject);
                            hazardGameObjectsMap[x, z] = null;
                        }
                    }
                }

                if (wasRowDeleted)
                {
                    var lastRowIndex = mod(z - 1, height);
                    createRow(
                        z,
                        groundGameObjectsMap[0, lastRowIndex].transform.position +
                        new Vector3(0, 0, stepSize) +
                        -new Vector3(0, 0, fieldSpeed * Time.deltaTime)
                    );

                    if (!inputQueue.IsEmpty())
                    {
                        isProcessingInputQueue = true;
                    }
                }
            }
        }
    }

    void createRow(int rowNumber, Vector3 startPosition)
    {
        for (int i = 0; i < width; ++i)
        {
            var prefab = groundObjectPrefabs[Random.Range(0, groundObjectPrefabs.Length)];
            var groundObj = Instantiate(
                prefab,
                startPosition + new Vector3(
                    i * stepSize,
                    0,
                    0
                ),
                transform.rotation
            );
            groundObj.transform.parent = transform;
            groundGameObjectsMap[i, rowNumber] = groundObj;

            if (Random.value <= 0.1)
            {
                var hazard = hazardObjectPrefabs[Random.Range(0, hazardObjectPrefabs.Length)];
                hazardGameObjectsMap[i, rowNumber] = Instantiate(
                    hazard,
                    groundObj.transform.position + new Vector3(0, groundObj.transform.localScale.y, 0),
                    groundObj.transform.rotation
                );
                hazardGameObjectsMap[i, rowNumber].transform.parent = transform;
            }
        }
    }
}
