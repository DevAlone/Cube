using System.Collections.Generic;
using UnityEngine;

public class FieldGenerator : MonoBehaviour
{
    public GameObject[] groundObjectPrefabs;
    public GameObject[] groundHazardObjectPrefabs;
    public GameObject[] hazardObjectPrefabs;
    public BonusEntry[] bonuses;
    public LimitedFloatValue hazardProbability;
    public LimitedFloatValue hazardInTheGroundProbability;
    // per row
    public float hazardProbabilityIncrementer;
    public float hazardInTheGroundProbabilityIncrementer;

    public void CreateRow(
            Field field,
            int rowNumber,
            Vector3 shiftPosition,
            bool spawnHazards = true
    )
    {
        var skipIndex = -1;
        if (spawnHazards)
        {
            var paths = field.FindEntriesToRow(rowNumber, field.playerPositionInMap, new HashSet<Vector2Int>());
            if (paths.Count > 0)
            {
                var randomEntry = paths[Random.Range(0, paths.Count)];
                skipIndex = randomEntry;
            }

            if (skipIndex >= 0)
            {
                var position = shiftPosition + new Vector3(
                    skipIndex * field.cellSize,
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
                if (field.objectsMap[0][rowNumber][skipIndex] != null)
                {
                    throw new System.Exception("trying to override an existing object");
                }
                field.objectsMap[0][rowNumber][skipIndex] = groundObj;
            }
            else
            {
                Debug.Log("Unable to find path");
            }
        }

        for (int i = 0; i < field.columns; ++i)
        {
            if (skipIndex == i)
            {
                continue;
            }

            var position = shiftPosition + new Vector3(
                i * field.cellSize,
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
            if (field.objectsMap[0][rowNumber][i] != null)
            {
                throw new System.Exception("trying to override an existing object");
            }
            field.objectsMap[0][rowNumber][i] = groundObj;

            if (spawnHazards && Random.value < hazardProbability)
            {
                var hazardPrefab = hazardObjectPrefabs[Random.Range(0, hazardObjectPrefabs.Length)];
                var hazard = Instantiate(
                    hazardPrefab,
                    groundObj.transform.position + new Vector3(0, groundObj.transform.localScale.y, 0),
                    groundObj.transform.rotation
                );
                hazard.transform.parent = transform;
                if (field.objectsMap[1][rowNumber][i] != null)
                {
                    throw new System.Exception("trying to override an existing object");
                }
                field.objectsMap[1][rowNumber][i] = hazard;
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
                        if (field.objectsMap[1][rowNumber][i] != null)
                        {
                            throw new System.Exception("trying to override an existing object");
                        }
                        field.objectsMap[1][rowNumber][i] = bonus;

                    }
                }
            }
        }

        field.onRowCreated?.Invoke();
    }
}
