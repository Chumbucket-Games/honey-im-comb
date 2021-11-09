using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] AnimationCurve spawnRate;
    [SerializeField] Enemy objectToSpawn;
    [SerializeField] BuildingType buildingType;
    [SerializeField] SquareGrid rallyPoint;

    private bool spawningEnabled = false;
    private float nextSpawnTime = 0.0f;

    EnemyWave currentWave;

    // Start is called before the first frame update
    void Start()
    {
        EnableSpawning();
    }

    // Update is called once per frame
    void Update()
    {
        if (spawningEnabled && Time.time >= nextSpawnTime)
        {
            Spawn();
            float currentSpawnRate = spawnRate.Evaluate(Time.time / 60f);
            nextSpawnTime = Time.time + (60f / currentSpawnRate);
        }
        if (currentWave.Attacking)
        {
            // Build a new enemy wave. I set to null to signal garbage collection on the previous wave before initialising the next one.
            currentWave = null;
            currentWave = new EnemyWave(5);
        }
    }

    private void Spawn()
    {
        var spawnedInstance = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
        spawnedInstance.AssignToWave(currentWave);

        if (!rallyPoint.IsGridFull)
        {
            var rallyGridCell = rallyPoint.GetNextAvailableCell();
            rallyGridCell.OccupyCell();

            // This rotation will turn the enemy to face the direction it is moving rather than having it face the same way as the rally point grid.
            Vector3 faceDirection = -(spawnedInstance.transform.position - rallyGridCell.Position).normalized;
            faceDirection.y = 0;
            spawnedInstance.Move(rallyGridCell.Position, Quaternion.FromToRotation(spawnedInstance.transform.forward, faceDirection));
        }
        else
        {
            // create a new grid or send the wave?
        }
    }

    public void EnableSpawning()
    {
        spawningEnabled = true;
        currentWave = new EnemyWave(5);
    }

    public void DisableSpawning()
    {
        spawningEnabled = false;
    }
}
